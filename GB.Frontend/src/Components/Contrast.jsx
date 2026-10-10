import { useState } from "react";

import Window from "./Window";

import { useEmulator } from "../Emulator";

export default function Contrast({ id, title, subtitle }) {
    const { gameboyRef } = useEmulator();

    const pos = [40, id * 30];

    const [contrast, setContrast] = useState(0.5);

    const changeContrast = value => {
        gameboyRef.current?.AdjustContrast(value - contrast);
        setContrast(value);
    };

    return (
        <Window title={title} subtitle={subtitle} id={id} position={pos}>
            <div className="h-full flex items-center justify-center">
                <div className="w-full max-w-sm">
                    <div className="flex justify-center mb-3 font-mono text-lg text-neutral-400">
                        {contrast.toFixed(2)}
                    </div>

                    <div className="relative h-8 flex items-center">
                        <div className="absolute inset-x-0 h-1 rounded-full bg-white/10" />
                        <div className="absolute left-0 h-1 rounded-full bg-white/30" style={{width: `${contrast * 100}%`}} />
                        <input type="range" min="0" max="1" step="0.01" value={contrast} onChange={e => changeContrast(Number(e.target.value))} className="absolute inset-0 w-full cursor-pointer opacity-0" />
                        <div className="absolute size-4 rounded-full border border-white/30 bg-neutral-800 shadow-md pointer-events-none" style={{left: `calc(${contrast * 100}% - 8px)` }} />
                    </div>
                </div>
            </div>
        </Window>
    );
}