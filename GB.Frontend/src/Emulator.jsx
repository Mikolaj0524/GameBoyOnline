import { createContext, useContext, useEffect, useRef, useState } from "react";
import { StatusType } from "./Utils/StatusType";
import Controls from "./Overlay";
import { IncludesBind as includesBind, LoadDefaultBinds as loadDefaultBinds } from "./Components/KeyBinds";

const { dotnet } = await new Function('return import("/wasm/dotnet.js")')();
const EmulatorContext = createContext();

export default function Emulator({ memory, setStatus }) {
	// Constants
    const FREQ = 4194304;
    const FRAME_CYCLES = 70224;
    const FRAME_TIME = (FRAME_CYCLES / FREQ) * 1000;
    const BUFFER_SIZE = 160 * 144 * 3;

	// Globals
    const gameboyRef = useRef(null);
    const runtimeRef = useRef(null);
    const runningRef = useRef(false);

	// Rendering
    const canvasRef = useRef(null);
    const bufferPtrRef = useRef(0);
    const imageRef = useRef(null);
    const animationFrameRef = useRef(null);

	// Sync
    const totalRef = useRef(0);
    const lastRef = useRef(0);

	// Audio
    const contextRef = useRef(null);
    const audioNodeRef = useRef(null);

    const checksumRef = useRef(null);

    useEffect(() => {
        initWasm();
        loadDefaultBinds();

        const handleKeyDown = e => {
            if (contextRef.current?.state === "suspended")
                contextRef.current.resume();

            if (!runningRef.current || !gameboyRef.current)
                return;

            if (e.code === "Equal")
                gameboyRef.current.AdjustContrast(0.1);

            if (e.code === "Minus")
                gameboyRef.current.AdjustContrast(-0.1);

            const b = includesBind(e.key);
            if (b !== -1)
                gameboyRef.current.SetButtonState(b, true);
        };

        const handleKeyUp = e => {
            if (!runningRef.current || !gameboyRef.current)
                return;

            const b = includesBind(e.key);
            if (b !== -1)
                gameboyRef.current.SetButtonState(b, false);
        };

        const saveRam = () => {
            if (!gameboyRef.current || !checksumRef.current || !runningRef.current)
                return;

            try {
                const bytes = gameboyRef.current.GetSaveRam ? gameboyRef.current.GetSaveRam() : gameboyRef.current.SaveRam();

                if (bytes && bytes.length > 0) {
                    const key = `save_${checksumRef.current}`;
                    localStorage.setItem(key, JSON.stringify(Array.from(bytes)));
                }
            } 
            catch (e) {
                console.error("Unable to save ram:", e);
            }
        };

        window.addEventListener("keydown", handleKeyDown);
        window.addEventListener("keyup", handleKeyUp);
        window.addEventListener("beforeunload", saveRam);
        const Interval = setInterval(saveRam, 5000);

        return () => {
            saveRam();
            clearInterval(Interval);
            window.removeEventListener("keydown", handleKeyDown);
            window.removeEventListener("keyup", handleKeyUp);
            window.removeEventListener("beforeunload", saveRam);

            if (audioNodeRef.current) {
                audioNodeRef.current.disconnect();
                audioNodeRef.current = null;
            }

            if (contextRef.current) {
                contextRef.current.close();
                contextRef.current = null;
            }
        };
    }, []);

    const initWasm = async () => {
        try {
            setStatus(["Initializing WASM.", StatusType.Info]);

            const runtime = await dotnet.withDiagnosticTracing(false).withApplicationArgumentsFromQuery().create();
            const { setModuleImports, getAssemblyExports, getConfig } = runtime;

            setModuleImports("main.js", {});
            const exports = await getAssemblyExports(getConfig().mainAssemblyName);

            runtimeRef.current = runtime;
            gameboyRef.current = exports.GB.WASM.Program;

            if (canvasRef.current) {
                const ctx = canvasRef.current.getContext("2d");
                imageRef.current = ctx.createImageData(160, 144);
            }

            gameboyRef.current.Init();
            setStatus(["Successfully initialized WASM!", StatusType.Success]);
            loadFiles();
        }
        catch (e) {
            console.error("WASM initialization error:", e);
            setStatus(["Unable to initialize WASM!", StatusType.Error]);
        }
    };

    const loadFiles = async () => {
        setStatus(["Loading BIOS file.", StatusType.Info]);
        const biosArr = new Uint8Array(await memory[0].arrayBuffer());
        gameboyRef.current.SetBios(biosArr);
        setStatus(["Successfully loaded BIOS file.", StatusType.Success]);

        setStatus(["Loading game rom!", StatusType.Info]);
        const romArr = new Uint8Array(await memory[1].arrayBuffer());
        gameboyRef.current.SetRom(romArr);
        setStatus(["Successfully loaded game rom.", StatusType.Success]);

        const buffer = await crypto.subtle.digest("SHA-256", romArr);
        const hash = Array.from(new Uint8Array(buffer));
        checksumRef.current = hash.map(b => b.toString(16).padStart(2, "0")).join("");

		initAudio();
        runEmulation();
    };

    const initAudio = async () => {
        const context = new AudioContext({ sampleRate: 48000 });

        await context.audioWorklet.addModule("/AudioProcessor.js");

        const node = new AudioWorkletNode(context, "audio", {
            numberOfInputs: 0,
            numberOfOutputs: 1,
            outputChannelCount: [2]
        });

        const bufferSize = 1024;

        const sendAudio = () => {
            if (!gameboyRef.current || !runtimeRef.current)
                return;

            gameboyRef.current.ReadAudioSamples();

            const ptr = gameboyRef.current.GetAudioBufferPtr();
            const r = runtimeRef.current;
            const heap = r.localHeapViewU8 ? r.localHeapViewU8() : r.Module.HEAPU8;

            const samples = new Float32Array(heap.buffer, ptr, bufferSize * 2);
            const copy = new Float32Array(samples);

            node.port.postMessage(copy, [copy.buffer]);
        };

        node.port.onmessage = event => {
            if (event.data?.type === "requestData")
                sendAudio();
        };

        node.connect(context.destination);

        contextRef.current = context;
        audioNodeRef.current = node;

        if (context.state === "running")
            await context.resume();

        for (let i = 0; i < 3; i++)
            sendAudio();
    };

    const runEmulation = () => {
        setStatus(["Starting emulation.", StatusType.Info]);
        if (!gameboyRef.current.Run()) {
			setStatus(["!", StatusType.Error]);
            return;
        }

        bufferPtrRef.current = gameboyRef.current.GetFrameBufferPtr();
        setStatus(["Emulation started!", StatusType.Success]);

        if (!runningRef.current) {
            runningRef.current = true;
            lastRef.current = performance.now();
            totalRef.current = 0;
            animationFrameRef.current = requestAnimationFrame(loop);
        }
    };

    const loop = now => {
        if (!runningRef.current)
            return;

        totalRef.current += Math.min(now - lastRef.current, 100);
        lastRef.current = now;

        let frames = 0;
        try {
            while (totalRef.current >= FRAME_TIME && frames < 2) {
                gameboyRef.current.StepCpu(FRAME_CYCLES);
                totalRef.current -= FRAME_TIME;
                frames++;
            }
        }
        catch (e) {
            console.error("Emulation error:", e);
            runningRef.current = false;
            setStatus(["Instruction error!", StatusType.Error]);
            return;
        }

        if (totalRef.current > FRAME_TIME * 2)
            totalRef.current = 0;

        if (frames > 0)
            draw();

        animationFrameRef.current = requestAnimationFrame(loop);
    };

    const draw = () => {
        if (!canvasRef.current || !bufferPtrRef.current || !imageRef.current || !runtimeRef.current)
            return;

        const r = runtimeRef.current;
        const heap = r.localHeapViewU8n? r.localHeapViewU8() : r.Module.HEAPU8;

        const src = heap.subarray(bufferPtrRef.current, bufferPtrRef.current + BUFFER_SIZE);
        const data = imageRef.current.data;
        for (let i = 0, j = 0; i < BUFFER_SIZE; i += 3, j += 4) {
            data[j] = src[i + 2];
            data[j + 1] = src[i + 1];
            data[j + 2] = src[i];
            data[j + 3] = 255;
        }

        const ctx = canvasRef.current.getContext("2d");
        ctx.putImageData(imageRef.current, 0, 0);
    };

    const [fullscreen, setFullscreen] = useState(false);

    return (
        <EmulatorContext.Provider value={{gameboyRef, runtimeRef}}>
            <div className={`w-full flex flex-col ${fullscreen ? "w-11/12" : "sm:w-11/12 md:w-3/4 lg:w-1/3"}`}>
                <div className="relative">
                    <div className={`relative ${fullscreen ? "absolute left-1/2 top-1/2 -translate-x-1/2 w-full aspect-10/9" : "w-full aspect-10/9"}`}>
                        <canvas ref={canvasRef} width={160} height={144} className="[image-rendering:pixelated] w-full h-full border border-white/10 rounded-sm" />
                        <button type="button" className="absolute top-2 right-2 p-1.5 bg-black/60 hover:bg-black/80 backdrop-blur-md border border-white/10 rounded-sm text-neutral-300 hover:text-white cursor-pointer flex items-center justify-center" onClick={() => setFullscreen((p) => !p)}>
                            <svg xmlns="http://www.w3.org/2000/svg" className="size-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
                                {fullscreen ? (
                                    <>
                                        <path d="M8 3v5H3" />
                                        <path d="M16 3v5h5" />
                                        <path d="M8 21v-5H3" />
                                        <path d="M16 21v-5h5" />
                                    </>
                                ) : (
                                    <>
                                        <path d="M3 8V3h5" />
                                        <path d="M21 8V3h-5" />
                                        <path d="M3 16v5h5" />
                                        <path d="M21 16v5h-5" />
                                    </>
                                )}
                            </svg>
                        </button>
                    </div>
                </div>
                {!fullscreen && <Controls />}
            </div>
        </EmulatorContext.Provider>
    );
}

export const useEmulator = () => useContext(EmulatorContext);