import { createContext, useContext, useEffect, useState } from "react";

import HexView from "./Components/HexView";
import Registers from "./Components/Registers";
import Timers from "./Components/Timers";
import Interrupts from "./Components/Interrupts";
import KeyBinds from "./Components/KeyBinds";
import Pad from "./Components/Pad";
import Contrast from "./Components/Contrast";

const OverlayContext = createContext();

export default function Controls() {
    const [tick, setTick] = useState(0);
    const [priority, setPriority] = useState([0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
    const [show, setShow] = useState([]);
    
    useEffect(() => {
        const interval = setInterval(() => setTick(t => t + 1), 50);
        return () => clearInterval(interval);
    }, []);
    
    const changePriority = id => setPriority(prev => [...prev.filter(v => v !== id), id]);

    const ELEMENTS = [
        {id: 0, title: "VRAM", subtitle: "0x8000 - 0x9FFF", component: HexView, dev: true},
        {id: 1, title: "WRAM", subtitle: "0xC000 - 0xDFF", component: HexView, dev: true},
        {id: 2, title: "HRAM", subtitle: "0xFF80 - 0xFFFE", component: HexView, dev: true},
        {id: 3, title: "OAM", subtitle: "0xFE00 - 0xFE9F", component: HexView, dev: true},
        {id: 4, title: "IO", subtitle: "0xFF00 - 0xFF7F", component: HexView, dev: true},
        {id: 5, title: "TIMERS", subtitle: "", component: Timers, dev: true},
        {id: 6, title: "IC", subtitle: "", component: Interrupts, dev: true},
        {id: 7, title: "REGS", subtitle: "", component: Registers, dev: true},
        
        {id: 8, title: "KEYS", subtitle: "", component: KeyBinds, dev: false},
        {id: 9, title: "CONTRAST", subtitle: "", component: Contrast, dev: true},
        {id: 10, title: "JOYPAD", subtitle: "", component: null, dev: false},
        {id: 11, title: "ADVANCED", subtitle: "", component: null, dev: false}
    ];

    return (
        <OverlayContext.Provider value={{priority, changePriority, tick, show, setShow}}>
            <div className="border-y border-white/10 my-4 py-4 flex gap-1 flex-wrap">
                {ELEMENTS.filter(item => item.dev ? show.includes(11) : true).map(item => <Item title={item.title} id={item.id} key={item.id} />)}
            </div>
            {show.includes(10) && <Pad />}
            <div className="fixed inset-0 z-50 pointer-events-none overflow-hidden">
                {ELEMENTS.filter(item => show.includes(item.id)).map(item => {
                    const Component = item.component;
                    if(!Component)
                        return;

                    return <Component key={item.id} id={item.id} title={item.title} subtitle={item.subtitle} />
                })}
            </div>
        </OverlayContext.Provider>
    );
}

function Item({title, id}){
    const {show, setShow} = useOverlay();

    const handleClick = () => {
        setShow(prev => {
            if (prev.includes(id))
                return prev.filter(v => v !== id);

            return [...prev, id];
        });
    }

    return(
        <div className={`${show.includes(id) ? "bg-white/20 border-white/30 text-white hover:bg-white/25" : "border-white/10 hover:bg-white/10 hover:border-white/20 hover:text-neutral-500"} text-neutral-500 min-w-20 flex-1 cursor-pointer rounded-md border p-1 text-center text-[12px] transition`} onClick={handleClick}>
            {title}
        </div>
    );
}

export const useOverlay = () => useContext(OverlayContext);