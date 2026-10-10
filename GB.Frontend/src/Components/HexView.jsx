import { useEffect, useRef, useState } from "react";
import Window from "./Window";
import { useOverlay } from "../Overlay";
import { useEmulator } from "../Emulator";

const StringArray = Array.from({length: 256}, (_, i) => i.toString(16).padStart(2, "0").toUpperCase());

export default function HexView({id, title, subtitle, memoryId}) {
    const {gameboyRef, runtimeRef} = useEmulator();
    const {tick} = useOverlay();

    const pos = [40, id * 30];

    const [memory, setMemory] = useState(null);
    const [error, setError] = useState(null);

    const prev = useRef(null);
    const flashArr = useRef(null);

    useEffect(() => {
        if (!gameboyRef.current || !runtimeRef.current) {
            setError("Waiting for WASM...");
            return;
        }

        const g = gameboyRef.current, r = runtimeRef.current;

        try {
            const heap = r.localHeapViewU8?.() || r.Module?.HEAPU8;
            const size = g.GetMemSize(memoryId);
            const ptr = g.GetMemPtr(memoryId) >>> 0;

            if (!heap || !size || (ptr + size > heap.length)) {
                setError("Wrong ptr or size address!");
                return;
            }

            setError(null);
            setMemory(heap.slice(ptr, ptr + size));
        } 
        catch (e) {
            setError(`Error: ${e.message}`);
        }
    }, [gameboyRef, runtimeRef, memoryId, tick]);


    useEffect(() => {
        if (!memory) 
            return;

        if (!prev.current || prev.current.length !== memory.length) {
            prev.current = new Uint8Array(memory);
            flashArr.current = new Uint8Array(memory.length);
            return;
        }

        for (let i = 0; i < memory.length; i++) {
            if (memory[i] !== prev.current[i]){
                flashArr.current[i] = 20;
            }
            else if (flashArr.current[i]){
                flashArr.current[i]--;
            }
        }

        prev.current.set(memory);
    }, [memory]);

    return (
        <Window id={id} title={title} subtitle={subtitle} position={pos}>
            <div className="font-mono text-xs leading-5 overflow-auto h-full flex-1 select-text p-2 min-h-0 text-white/80">
                {error ? (
                    <div className="text-yellow-400 p-1">{error}</div>
                ) : memory ? (
                    <div className="flex flex-wrap gap-x-1">
                        {Array.from(memory, (val, i) => (
                            <span key={i} className={flashArr.current?.[i] && "text-yellow-400 font-bold"}>
                                {StringArray[val]}
                            </span>
                        ))}
                    </div>
                ) : (
                    <div className="text-white/40">Waiting for data...</div>
                )}
            </div>
        </Window>
    );
}