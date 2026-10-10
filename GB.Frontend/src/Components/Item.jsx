export default function Item({name, value, places = 2}) {
    return (
        <div className="flex flex-1 items-center justify-between px-3 py-2.5 min-w-30 font-mono bg-white/4 border border-white/6 rounded-lg mb-2">
            <span className="text-gray-300 text-xs font-medium tracking-wide">{name}</span>
            <span className="text-amber-300 text-xs font-semibold">{places > 1 && "0x"}{places == 0 ? value : (places == 1 ? (value ? "True" : "False") : value.toString(16).padStart(places, "0").toUpperCase())}</span>
        </div>
    );
}