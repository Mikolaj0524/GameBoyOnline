
import { useEffect, useState } from "react";
import { verifyAccess, getList, getFile } from "../api";

export default function FilesSelect({ setMemory }) {
    const [bios, setBios] = useState(null);
    const [rom, setRom] = useState(null);

    const handleClick = () => {
        if (!bios || !rom)
            return;

        setMemory([bios, rom]);
        setBios(null);
        setRom(null);
    };

    return (
        <div className="flex flex-col gap-4 w-full max-w-md">
            <FileSlot text="BIOS" type="bios" setFile={setBios} />
            <FileSlot text="ROM" type="rom" setFile={setRom} />

            {bios && rom && (
                <div className="flex flex-col gap-2 rounded-lg border border-white/20 bg-neutral-900/90 p-3.5 shadow-xl backdrop-blur-md text-xs font-mono w-full">
                    <button type="button" className="cursor-pointer px-3 py-2 rounded-md border border-white/15 bg-neutral-800/80 text-center text-neutral-300 hover:bg-neutral-700 hover:border-white/30 transition-all" onClick={handleClick}>
                        Run
                    </button>
                </div>
            )}
        </div>
    );
}

function FileSlot({ text, type, setFile }) {
    const [name, setName] = useState("");
    const [list, setList] = useState([]);
    const [open, setOpen] = useState(false);
    const [search, setSearch] = useState("");
    const [loading, setLoading] = useState(false);
    const [verified, setVerified] = useState(false);
    const [error, setError] = useState("");
    const [selectedId, setSelectedId] = useState(null);

    useEffect(() => {
        if (!open || !verified || list.length > 0)
            return;

        let cancelled = false;

        setLoading(true);
        setError("");

        getList(type)
            .then(items => {
                if (!cancelled)
                    setList(items);
            })
            .catch(err => {
                if (!cancelled) {
                    setError(err.message);
                    setList([]);
                }
            })
            .finally(() => {
                if (!cancelled)
                    setLoading(false);
            });

        return () => cancelled = true;
    }, [open, verified, type, list.length]);

    const handleSearchOpen = async () => {
        if (open) {
            setOpen(false);
            return;
        }

        setError("");

        const existing = localStorage.getItem("token");
        if (existing) {
            setVerified(true);
            setOpen(true);
            return;
        }

        localStorage.removeItem("role");
        let code = localStorage.getItem("code");
        if (!code) {
            code = prompt("Please enter access code:");
            if (code === null || !code.trim())
                return;
            code = code.trim();
        }

        try {
            setLoading(true);

            const token = await verifyAccess(code);
            if (!token) {
                localStorage.removeItem("code");
                localStorage.removeItem("token");
                localStorage.removeItem("role");

                setVerified(false);
                setList([]);
                setError("Wrong access code.");
                alert("Wrong access code. Please try again.");
                return;
            }

            localStorage.setItem("code", code);

            setVerified(true);
            setOpen(true);
        } 
        catch (err) {
            console.error("Authentication error:", err);
            localStorage.removeItem("token");
            localStorage.removeItem("role");
            localStorage.removeItem("code");

            setVerified(false);
            setError(err.message || "Unable to verify access code.");
        } 
        finally {
            setLoading(false);
        }
    };

    const handleSelect = async item => {
        try {
            setLoading(true);
            setError("");

            const file = await getFile(type, item.id);
            const fileName = `${item.name || `file-${item.id}`}${item.ext || ""}`;

            setFile(new File([file], fileName, { type: "application/octet-stream" }));
            setName(fileName);
            setSelectedId(item.id);
            setOpen(false);
        } 
        catch (err) {
            console.error("File download error:", err);
            setError(err.message);
        } 
        finally {
            setLoading(false);
        }
    };

    const handleUpload = e => {
        const file = e.target.files?.[0];

        if (!file)
            return;

        setFile(file);
        setName(file.name);
        setSelectedId(null);
        setError("");
        e.target.value = "";
    };

    const filtered = list.filter(item => String(item.name).toLowerCase().includes(search.toLowerCase()));

    return (
        <div className="flex flex-col gap-2 rounded-lg border border-white/20 bg-neutral-900/90 p-3.5 shadow-xl backdrop-blur-md text-xs font-mono w-full">
            <div className="flex items-center justify-between gap-2 border-b border-white/10 pb-2">
                <span className="font-bold text-white tracking-wider bg-white/10 px-2 py-0.5 rounded text-[11px]">
                    SELECT {text}
                </span>

                <span className={`truncate text-right max-w-50 ${name ? "text-emerald-400 font-semibold" : "text-neutral-500 italic"}`}>
                    {name || "No file..."}
                </span>
            </div>

            <div className="grid grid-cols-2 gap-2 pt-1">
                <button type="button" className={`cursor-pointer px-3 py-2 rounded-md border text-center transition-all ${open ? "bg-white/20 border-white/50 text-white font-bold" : "bg-neutral-800/80 border-white/15 text-neutral-300 hover:bg-neutral-700 hover:border-white/30"}`} onClick={handleSearchOpen} disabled={loading}>
                    {loading ? "Loading..." : open ? "Close" : "Search"}
                </button>

                <label className="cursor-pointer px-3 py-2 rounded-md border border-white/15 bg-neutral-800/80 text-center text-neutral-300 hover:bg-neutral-700 hover:border-white/30 transition-all">
                    Upload file
                    <input type="file" accept=".gb,.gbc,.bin,.rom" onChange={handleUpload} className="hidden" />
                </label>
            </div>

            {error && (
                <div className="text-red-400 py-1 text-center">
                    {error}
                </div>
            )}

            {open && (
                <div className="mt-2 flex flex-col gap-2 pt-2 border-t border-white/10">
                    <input type="text" className="w-full bg-neutral-950 border border-white/20 rounded px-2.5 py-1.5 text-xs text-neutral-200 placeholder-neutral-500 focus:outline-none focus:border-white/50" value={search} onChange={e => setSearch(e.target.value)} placeholder="Search list..." />

                    {loading && (
                        <div className="text-center text-neutral-400 py-2">
                            Loading...
                        </div>
                    )}

                    {!loading && (
                        <ul className="max-h-36 overflow-y-auto space-y-1 rounded bg-neutral-950/60 p-1 border border-white/10">
                            {filtered.length > 0 ? (
                                filtered.map(item => (
                                    <li key={item.id}>
                                        <button type="button" className={`w-full text-left px-2 py-1.5 rounded text-neutral-300 hover:bg-white/15 hover:text-white transition-colors truncate ${selectedId === item.id ? "bg-white/10 text-white" : ""}`} onClick={() => handleSelect(item)}>
                                            {item.name}
                                        </button>
                                    </li>
                                ))
                            ) : (
                                <li className="px-2 py-2 text-center text-neutral-500 text-[11px]">
                                    No results
                                </li>
                            )}
                        </ul>
                    )}
                </div>
            )}
        </div>
    );
}
