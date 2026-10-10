import { useEmulator } from "../Emulator";

export default function Pad() {
    const cross = [
        [2, "left-1/3 top-0 w-1/3 h-1/3 rounded-t border-b-0", "rotate-90"],
        [3, "left-1/3 bottom-0 w-1/3 h-1/3 rounded-b border-t-0", "-rotate-90"],
        [1, "left-0 top-1/3 w-1/3 h-1/3 rounded-l border-r-0", ""],
        [0, "right-0 top-1/3 w-1/3 h-1/3 rounded-r border-l-0", "rotate-180"]
    ];

    return (
        <div className="mt-3 text-neutral-500">
            <div className="flex items-center justify-between px-2 sm:px-6">
                <div className="relative size-28 sm:size-32">
                    {cross.map(item => (
                        <Button key={item[0]} button={item[0]} classes={`absolute ${item[1]} border-white/10 bg-white/6 hover:bg-white/9 active:bg-white/12`}>
                            <span className={`block ${item[2]}`}>◀</span>
                        </Button>
                    ))}

                    <div className="absolute left-1/3 top-1/3 w-1/3 h-1/3 bg-white/6" />
                </div>
                <div className="flex gap-4 sm:gap-6">
                    <Button button={5} classes="-mb-9 size-14 sm:size-16 rounded-full border border-white/10 bg-white/6 font-bold text-sm hover:bg-white/9 active:bg-white/12">B</Button>
                    <Button button={4} classes="-mt-9 size-14 sm:size-16 rounded-full border border-white/10 bg-white/6 font-bold text-sm hover:bg-white/9 active:bg-white/12">A</Button>
                </div>
            </div>
            <div className="flex justify-center gap-5 mt-10">
                {[[6, "SELECT"], [7, "START"]].map(item => (
                    <Button key={item[0]} button={item[0]} classes="group flex flex-col items-center gap-1 -rotate-15">
                        <span className="w-14 sm:w-16 h-4 rounded-full border border-white/10 bg-white/5 group-hover:bg-white/8 group-active:bg-white/12" />
                        <span className="text-[9px] tracking-widest">{item[1]}</span>
                    </Button>
                ))}
            </div>
        </div>
    );
}

function Button({button, classes, children}) {
    const {gameboyRef} = useEmulator();
    if (!gameboyRef.current)
        return null;

    return (
        <button type="button" className={`touch-none select-none transition active:scale-98 ${classes}`}
            onPointerDown={e => {
                e.preventDefault();
                e.currentTarget.setPointerCapture(e.pointerId);
                gameboyRef.current?.SetButtonState(button, true);
            }}
            onPointerUp={e => {
                e.preventDefault();
                gameboyRef.current?.SetButtonState(button, false);
            }}
        >
            {children}
        </button>
    );
}