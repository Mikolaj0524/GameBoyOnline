
import { useEffect, useState } from "react";
import { StatusType } from "./Utils/StatusType";
import Footer from "./Components/Footer";
import Header from "./Components/Header";
import FilesSelect from "./Components/FilesSelect";
import RomManager from "./Components/RomManager";
import Emulator from "./Emulator";
import { verifyAccess } from "./api";

export default function App() {
    const [status, setStatus] = useState(["...", StatusType.Default]);
    const [memory, setMemory] = useState(null);
    const [view, setView] = useState("emulator");
    const [isSpecial, setIsSpecial] = useState(localStorage.getItem("role") === "Special");

    useEffect(() => {
        const updateRole = () => setIsSpecial(localStorage.getItem("role") === "Special");
        window.addEventListener("auth-changed", updateRole);
        return () => window.removeEventListener("auth-changed", updateRole);
    }, []);

    const toggleView = async () => {
        if (view === "rom-manager") {
            setView("emulator");
            return;
        }

        if (isSpecial) {
            setView("rom-manager");
            return;
        }

        const code = window.prompt("Enter your access key:");
        if (!code)
            return;

        try {
            const token = await verifyAccess(code);
            if (!token) {
                window.alert("Invalid access key.");
                return;
            }

            const role = localStorage.getItem("role");
            if (role !== "Special") {
                window.alert("This feature requires a Special access key.");
                return;
            }

            setIsSpecial(true);
            setView("rom-manager");
        } 
        catch (error) {
            window.alert(error.message || "Failed to verify access key.");
        }
    };

    return (
        <div className="min-h-dvh w-full bg-[#0a0a0c] text-neutral-300 font-mono flex flex-col items-center justify-between p-4 sm:p-8 select-none relative overflow-hidden">
            <div className="pointer-events-none absolute inset-0 opacity-[0.05] bg-size-[24px_24px] bg-[radial-gradient(#fff_1px,transparent_1px)]" />
            <Header status={status} />

            <main className="relative z-10 my-auto py-8 flex flex-col items-center justify-center w-full">
                {view === "rom-manager" ? (
                    <RomManager />
                ) : memory ? (
                    <Emulator setStatus={setStatus} memory={memory} />
                ) : (
                    <FilesSelect setMemory={setMemory} />
                )}
            </main>

            <Footer view={view} onToggleView={toggleView} />
        </div>
    );
}
