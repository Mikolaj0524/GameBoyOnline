import BindItem from "./BindItem";
import Window from "./Window";

export default function KeyBinds({id}){
    const pos = [40, id * 30];

    return(
        <Window id={id} title="Key binds" subtitle="" position={pos}>
            <div className="flex gap-2 flex-wrap items-center justify-center">
                <BindItem name="Left" />
                <BindItem name="Up" />
                <BindItem name="Down" />
                <BindItem name="Right" />
            </div>
            <div className="flex gap-2 flex-wrap items-center justify-center">
                <BindItem name="A" />
                <BindItem name="B" />
                <BindItem name="Start" />
                <BindItem name="Select" />
            </div>
        </Window>
    );
};

export const binds = [
    ["Right", "ArrowRight"],
    ["Left", "ArrowLeft"],
    ["Up", "ArrowUp"],
    ["Down", "ArrowDown"],
    ["A", ","],
    ["B", "."],
    ["Select", "Shift"],
    ["Start", "Enter"]
];

export function LoadDefaultBinds(){
    binds.map(item => {
        const keyId = `key_${item[0]}`;

        const val = localStorage.getItem(keyId);
        if(!val){
            localStorage.setItem(keyId, item[1]);
        }
    });
}

export function IncludesBind(key) {
    return binds.findIndex(item => {
        const keyId = `key_${item[0]}`;

        const val = localStorage.getItem(keyId);
        return val === key;
    });
}