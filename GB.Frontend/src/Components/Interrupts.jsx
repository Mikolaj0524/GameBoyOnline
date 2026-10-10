import Window from "./Window";
import Item from "./Item";
import { useEmulator } from "../Emulator";

export default function Interrupts({id}) {
    const {gameboyRef} = useEmulator();
    
    const pos = [40, id * 30];

    const interrupts = gameboyRef?.current?.GetInterrupts?.() ?? null;
    if (!interrupts)
        return;

    return (
        <Window title="Interrupt Controller" subtitle="" id={id} position={pos}>
            <div className="flex gap-2 flex-wrap items-center justify-center">
                <Item name="IE" value={interrupts[0] ?? 0} />
                <Item name="IF" value={interrupts[1] ?? 0} />
            </div>

            <div className="space-y-1">
                {["VBlank", "STAT", "Timer", "Serial", "Joypad"].map((name, idx) => (
					<Item name={name} value={`IE: ${((interrupts[0] ?? 0) & (1 << idx)) !== 0 ? "1" : "0"} | IF: ${((interrupts[1] ?? 0) & (1 << idx)) !== 0 ? "1" : "0"}`} places={0} key={idx} />
                ))}
            </div>
        </Window>
    );
}