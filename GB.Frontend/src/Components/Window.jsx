import { useEffect, useRef, useState } from "react";

import { useOverlay } from "../Overlay";

export default function Window({id, title, subtitle, position = [100, 100], children}) {
    const { priority, changePriority } = useOverlay();

    const [pos, setPos] = useState(null);
    const [size, setSize] = useState(null);
    const [expanded, setExpanded] = useState(true);

    const moveRef = useRef(null);
    const resizeRef = useRef(null);

    const windowId = `window_${id}`;

    const loadDefault = () => {
        setPos(position);
        setSize([600, 250]);
    }

    useEffect(() => {
        const val = localStorage.getItem(windowId);

        if (val) {
            const [p, s] = JSON.parse(val);
            const w = window.innerWidth;
            const h = window.innerHeight;
            const padding = 10;

            if (p[0] + s[0] < padding || p[0] > w - padding || p[1] + s[1] < padding || p[1] > h - padding)
                loadDefault();
            else {
                setPos(p);
                setSize(s);
            }
        }
        else{
            loadDefault();
        }

        const handlePointerMove = e => {
            if (moveRef.current) {
                const [id, x, y, p] = moveRef.current;

                if (e.pointerId !== id)
                    return;

                setPos([p[0] + e.clientX - x, p[1] + e.clientY - y]);
            }

            if (resizeRef.current) {
                const [id, x, y, s] = resizeRef.current;

                if (e.pointerId !== id)
                    return;

                setSize([
                    Math.max(250, s[0] + e.clientX - x),
                    Math.max(100, s[1] + e.clientY - y)
                ]);
            }
        };

        const handlePointerUp = e => {
            if (moveRef.current?.[0] === e.pointerId)
                moveRef.current = null;

            if (resizeRef.current?.[0] === e.pointerId)
                resizeRef.current = null;
        };

        window.addEventListener("pointermove", handlePointerMove);
        window.addEventListener("pointerup", handlePointerUp);
        window.addEventListener("pointercancel", handlePointerUp);

        return () => {
            window.removeEventListener("pointermove", handlePointerMove);
            window.removeEventListener("pointerup", handlePointerUp);
            window.removeEventListener("pointercancel", handlePointerUp);
        };
    }, []);

    useEffect(() => {
        if (pos != null)
            localStorage.setItem(windowId, JSON.stringify([pos, size]));
    }, [windowId, pos, size]);

    const focus = () => changePriority(id);

    const handleStart = (e, move) => {
        e.preventDefault();
        e.stopPropagation();
        e.currentTarget.setPointerCapture(e.pointerId);
        focus();

        if (move){
            moveRef.current = [e.pointerId, e.clientX, e.clientY, pos];
        }
        else{
            resizeRef.current = [e.pointerId, e.clientX, e.clientY, size];
        }
    };

    if (pos == null)
        return null;

    return (
        <div className="absolute pointer-events-auto select-none overflow-hidden rounded-lg border border-white/10 bg-neutral-950/70 text-white shadow-lg backdrop-blur-xl flex flex-col" style={{ left: pos[0], top: pos[1], width: size[0], height: expanded ? size[1] : "auto", zIndex: priority.indexOf(id) + 10 }} onClick={focus} >
            <div className="shrink-0 flex items-center gap-2 border-b border-white/10 bg-white/3 px-3 py-2">
                <button type="button" className={`flex size-5 items-center justify-center rounded text-[10px] text-neutral-500 transition hover:bg-white/10 hover:text-white ${expanded ? "" : "-rotate-90"}`} onClick={() => setExpanded(v => !v)} >
                    ▲
                </button>

                <div className="flex min-w-0 flex-1 cursor-grab touch-none items-center justify-between gap-3 active:cursor-grabbing" onPointerDown={e => handleStart(e, true)} >
                    <span className="truncate text-xs font-semibold tracking-wide">
                        {title}
                    </span>

                    {subtitle && (
                        <span className="truncate text-[10px] font-mono text-neutral-500">
                            {subtitle}
                        </span>
                    )}
                </div>
            </div>

            {expanded && (
                <div className="relative flex-1 min-h-0 overflow-hidden bg-black/20 p-3">
                    {children}
                    <div className="absolute bottom-0 right-0 flex size-4 cursor-se-resize items-end justify-end p-0.5 text-neutral-600 hover:text-neutral-300" onPointerDown={e => handleStart(e, false)} >
                        <svg viewBox="0 0 10 10" className="size-2.5">
                            <line x1="9" y1="1" x2="1" y2="9" stroke="currentColor" strokeWidth="1.5" />
                            <line x1="9" y1="5" x2="5" y2="9" stroke="currentColor" strokeWidth="1.5" />
                        </svg>
                    </div>
                </div>
            )}
        </div>
    );
}