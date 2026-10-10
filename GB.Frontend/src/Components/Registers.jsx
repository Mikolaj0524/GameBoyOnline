import Window from "./Window";
import Item from "./Item";
import { useEmulator } from "../Emulator";

export default function Registers({id}) {
    const {gameboyRef} = useEmulator();

    const pos = [40, id * 30];

    const registers = gameboyRef?.current?.GetRegisters?.() ?? null;
    if(!registers)
        return;

    return (
        <Window title="CPU Registers" subtitle="" id={id} position={pos}>
            <div className="text-white/70 font-semibold my-1 text-xs">16-Bit Registers</div>
            <div className="flex gap-2 flex-wrap items-center justify-center">
                <Item name="AF" places={4} value={((registers[0] << 8) | registers[1]) ?? "??"} />
                <Item name="BC" places={4} value={((registers[2] << 8) | registers[3]) ?? "??"} />
                <Item name="DE" places={4} value={((registers[4] << 8) | registers[5]) ?? "??"} />
                <Item name="HL" places={4} value={((registers[6] << 8) | registers[7]) ?? "??"} />
            </div>
            <div className="text-white/70 font-semibold my-1 text-xs">8-Bit Registers</div>
            <div className="flex gap-2 flex-wrap items-center justify-center">
                <Item name="A" value={registers[0] ?? "??"} />
                <Item name="F" value={registers[1] ?? "??"} />
                <Item name="B" value={registers[2] ?? "??"} />
                <Item name="C" value={registers[3] ?? "??"} />
                <Item name="D" value={registers[4] ?? "??"} />
                <Item name="E" value={registers[5] ?? "??"} />
                <Item name="H" value={registers[6] ?? "??"} />
                <Item name="L" value={registers[7] ?? "??"} />
            </div>
            <div className="text-white/70 font-semibold my-1 text-xs">Pointers & Control</div>
            <div className="flex gap-2 flex-wrap items-center justify-center">
                <Item name="SP" places={4} value={((registers[8] << 8) | registers[9]) ?? "??"} />
                <Item name="PC" places={4} value={((registers[10] << 8) | registers[11]) ?? "??"} />
                <Item name="IME" places={1} value={registers[12] !== 0 ? 1 : 0} />
            </div>
            <div className="text-white/70 font-semibold my-1 text-xs">CPU Flags</div>
            <div className="flex gap-2 flex-wrap items-center justify-center">
                <Item name="Z" places={1} value={(registers[1] & 0x80) !== 0 ? 1 : 0} />
                <Item name="N" places={1} value={(registers[1] & 0x40) !== 0 ? 1 : 0} />
                <Item name="H" places={1} value={(registers[1] & 0x20) !== 0 ? 1 : 0} />
                <Item name="C" places={1} value={(registers[1] & 0x10) !== 0 ? 1 : 0} />
            </div>
        </Window>
    );
}