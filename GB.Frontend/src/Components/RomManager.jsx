import { useEffect, useState } from "react";
import { getList, uploadFile, deleteFile } from "../api";

const PAGE_SIZE = 10;

export default function RomManager() {
    const [roms, setRoms] = useState([]);
    const [loading, setLoading] = useState(false);
    const [uploading, setUploading] = useState(false);
    const [error, setError] = useState("");
    const [search, setSearch] = useState("");
    const [page, setPage] = useState(1);

    const refresh = async () => {
        setLoading(true);
        setError("");

        try {
            const result = await getList("rom");
            setRoms(Array.isArray(result) ? result : result.items ?? []);
        } 
        catch (err) {
            setError(err.message || "Failed to load ROMs.");
        } 
        finally {
            setLoading(false);
        }
    }

    useEffect(() => {
        refresh();
    }, []);

    const query = search.trim().toLowerCase();
    const filteredRoms = roms.filter(rom => `${rom.name ?? ""}${rom.ext ?? ""}`.toLowerCase().includes(query));

    const total = Math.max(1, Math.ceil(filteredRoms.length / PAGE_SIZE));
    const current = Math.min(page, total);
    const start = (current - 1) * PAGE_SIZE;
    const visible = filteredRoms.slice(start, start + PAGE_SIZE);

    useEffect(() => {
        if (page !== current)
            setPage(current);
    }, [page, current]);

    const handleUpload = async e => {
        const files = Array.from(e.target.files ?? []);
        e.target.value = "";
        if (!files.length)
            return;

        setUploading(true);
        setError("");

        try {
            for (const file of files)
                await uploadFile("rom", file);

            await refresh();
            setPage(1);
        } 
        catch (err) {
            setError(err.message || "Failed to upload ROMs.");
            await refresh();
        } 
        finally {
            setUploading(false);
        }
    }

    const handleDelete = async rom => {
        if (!window.confirm(`Delete "${rom.name}${rom.ext || ""}"?`))
            return;

        setError("");

        try {
            await deleteFile("rom", rom.id);
            await refresh();
        } 
        catch (err) {
            setError(err.message || "Failed to delete ROM.");
        }
    }


    return (
        <section className="flex w-full max-w-3xl flex-col overflow-hidden rounded-lg border border-white/10 bg-neutral-950/70 text-white shadow-lg backdrop-blur-xl">
            <div className="flex flex-col gap-3 border-b border-white/10 bg-black/20 p-3 sm:flex-row">
                <input type="search" value={search} onChange={e => { setSearch(e.target.value); setPage(1); }} placeholder="Search ROMs..." className="min-w-0 flex-1 rounded border border-white/10 bg-black/20 px-3 py-2 text-xs text-neutral-200 outline-none placeholder:text-neutral-600 transition focus:border-white/25" />
                <div className="flex shrink-0 gap-2">
                    <label className={`"flex items-center justify-center rounded border border-white/10 bg-white/3 px-3 py-2 text-[10px] text-neutral-400 transition hover:bg-white/8 hover:text-white disabled:cursor-not-allowed disabled:opacity-40" cursor-pointer ${uploading ? "pointer-events-none opacity-40" : ""}`}>
                        {uploading ? "Uploading..." : "+ Add ROMs"}
                        <input type="file" accept=".gb,.gbc,.bin,.rom" multiple onChange={handleUpload} disabled={uploading} className="hidden" />
                    </label>
                    <button type="button" onClick={refresh} disabled={loading} className={"flex items-center justify-center rounded border border-white/10 bg-white/3 px-3 py-2 text-[10px] text-neutral-400 transition hover:bg-white/8 hover:text-white disabled:cursor-not-allowed disabled:opacity-40"} >
                        {loading ? "Loading..." : "Refresh"}
                    </button>
                </div>
            </div>

            <div className="min-h-0 flex-1 bg-black/10 p-3">
                {error && (
                    <div className="mb-3 rounded border border-red-400/20 bg-red-400/5 px-3 py-2 text-[11px] text-red-300">
                        {error}
                    </div>
                )}

                {loading && roms.length === 0 ? (
                    <div className="py-12 text-center text-xs text-neutral-500">
                        Loading ROMs...
                    </div>
                ) : visible.length === 0 ? (
                    <div className="py-12 text-center text-xs text-neutral-500">
                        {search ? "No matching ROMs." : "No ROMs found."}
                    </div>
                ) : (
                    <ul className="flex flex-col gap-1">
                        {visible.map(rom => (
                            <li key={rom.id} className="group flex min-w-0 items-center gap-3 rounded border border-transparent px-2 py-2 transition hover:border-white/10 hover:bg-white/4" >
                                <div className="flex size-8 shrink-0 items-center justify-center rounded border border-white/10 bg-white/3 text-[9px] font-bold text-neutral-500">
                                    GB
                                </div>
                                <div className="min-w-0 flex-1">
                                    <p className="truncate text-[11px] text-neutral-200">
                                        {rom.name}{rom.ext || ""}
                                    </p>
                                    <p className="mt-0.5 text-[9px] text-neutral-600">
                                        ID: {rom.id}
                                    </p>
                                </div>
                                <button type="button" onClick={() => handleDelete(rom)} className="shrink-0 rounded border border-transparent px-2 py-1.5 text-[10px] text-neutral-600 transition hover:border-red-400/20 hover:bg-red-400/6 hover:text-red-300" >
                                    Delete
                                </button>
                            </li>
                        ))}
                    </ul>
                )}
            </div>
        </section>
    );
}