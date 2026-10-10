
export default function Footer({ view, onToggleView }) {
    return (
        <footer className="relative z-10 w-full max-w-xl pt-4 border-t border-neutral-800/80 flex flex-wrap items-center justify-between gap-3 text-[10px] text-neutral-500 tracking-wider">
            <span>LR35902 @ 4.19 MHz</span>
            <span>160×144 LCD</span>

            <button type="button" onClick={onToggleView} className="cursor-pointer transition-colors hover:text-white">
                {view === "rom-manager" ? "BACK TO EMULATOR" : "ROM MANAGER"}
            </button>
        </footer>
    );
}
