import { useEffect, useRef, useState } from "react";

export default function BindItem({name}) {
    const bindRef = useRef();

    const [recording, setRecording] = useState(false);
    const [key, setKey] = useState("None");

    const keyId = `key_${name}`;

    useEffect(() => {
        const val = localStorage.getItem(keyId);
        setKey(val ?? "None");
    }, []);

    useEffect(() => {
        const handleKey = e => {
            if (!recording)
                return;

            if(e.key == "Backspace" || e.key == "Escape"){
                setRecording(false);
                return;
            }

            if(e.key >= 0 && e.key <= 9 || e.key == "Minus" || e.key == "Equal")
                return;

            setKey(e.key);
            localStorage.setItem(keyId, e.key);
            setRecording(false);
        }

        const handleClick = e => {
            if (bindRef.current?.contains(e.target))
                return;

            setRecording(false);
        }

        window.addEventListener("keydown", handleKey);
        window.addEventListener("pointerdown", handleClick);
        return () => {
            window.removeEventListener("keydown", handleKey);
            window.removeEventListener("pointerdown", handleClick);
        }
    }, [recording, keyId]);

    return(
        <div className="flex flex-1 gap-2 flex-col flex-wrap items-center justify-center">
            <div className="text-white/70 font-semibold my-1 text-xs">{name}</div>

            <div ref={bindRef} className="cursor-pointer flex flex-1 items-center justify-center px-3 py-2.5 min-w-30 font-mono bg-white/4 border border-white/6 rounded-lg mb-2" onClick={() => setRecording(!recording)}>
                <span className="text-amber-300 text-xs font-semibold">{recording ? "Recording..." : key}</span>
            </div>
        </div>
    );
}