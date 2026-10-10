import { StatusType } from "../Utils/StatusType";

export default function Header({status}){

    const getStyle = type => {
        switch (type) {
            case StatusType.Error: return "text-red-400 border-red-900/50 bg-red-950/20";
            case StatusType.Info: return "text-amber-300 border-amber-900/50 bg-amber-950/20";
            case StatusType.Success: return "text-[#9bbc0f] border-[#8b956d]/40 bg-[#9bbc0f]/10";
            default: return "text-neutral-400 border-neutral-800 bg-neutral-900/60";
        }
    };

    return(
        <header className="relative z-10 w-full max-w-xl flex items-center justify-between gap-4 pb-4 border-b border-neutral-800/80">
            <div className="flex items-center gap-3">
                <span className="text-xs font-bold tracking-[0.25em] text-neutral-100">
                    GAME BOY
                </span>
                <span className="text-[10px] text-neutral-500 border border-neutral-800 px-1.5 py-0.5 rounded">
                    DMG
                </span>
            </div>

            <div className={`text-xs px-2.5 py-1 rounded border flex items-center gap-2 transition-all duration-200 ${getStyle(status[1])}`}>
                <span className="w-1.5 h-1.5 rounded-full bg-current animate-pulse" />
                <span className="truncate max-w-45 sm:max-w-60 text-[11px]">
                    {status[0]}
                </span>
            </div>
        </header>
    );
}