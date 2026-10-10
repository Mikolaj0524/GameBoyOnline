import Window from "./Window";
import Item from "./Item";
import { useEmulator } from "../Emulator";

export default function Timers({id}) {
    const {gameboyRef} = useEmulator();

    const pos = [40, id * 30];
    
    const timers = gameboyRef?.current?.GetTimers?.() ?? null;
    if(!timers)
        return;

    return (
        <Window title="Hardware Timers" subtitle="0xFF04 - 0xFF07" id={id} position={pos}>
            <Item name="DIV" value={timers[0] ?? "??"} />
            <Item name="TIMA" value={timers[1] ?? "??"} />
            <Item name="TMA" value={timers[2] ?? "??"} />
            <Item name="TAC" value={timers[3] ?? "??"} />
        </Window>
    );
}