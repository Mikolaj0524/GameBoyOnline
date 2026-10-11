<div align="center">

  <h1>🧠 GB — Emulator Core</h1>

  <p><b>A standalone Game Boy emulation engine written in C#.</b></p>
  <p>Emulation of the Sharp LR35902 CPU, PPU, APU, memory bus, timers and cartridge hardware.</p>

  <p>
    <img src="https://img.shields.io/badge/C%23-.NET%2010-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt="C# .NET 10" />
    <img src="https://img.shields.io/badge/Platform-Independent-2F855A?style=for-the-badge" alt="Platform independent" />
  </p>

  <p>
    <a href="#-overview">Overview</a> •
    <a href="#-core-components">Components</a> •
    <a href="#-architecture">Architecture</a> •
    <a href="#-mbc-support">MBC Support</a> •
    <a href="#-project-structure">Project Structure</a> •
    <a href="#-getting-started">Getting Started</a>
  </p>

</div>

---

## 📖 Overview

**GB** is the standalone emulation core powering GameBoyOnline. Built with **C# and .NET 10**, it implements the hardware systems required to run Game Boy software. The library is designed to be shared across different frontends without depending on a particular UI framework.

| Frontend | Description |
|:--|:--|
| **`GB.WASM`** | Browser-based emulator using WebAssembly |
| **`GB.WinForms`** | Native Windows desktop frontend |

### Key Features

- 🧠 **CPU**: Sharp LR35902 instruction execution and registers.
- 🎮 **Graphics**: Background, window and sprite rendering.
- 🔊 **Audio**: Four-channel Game Boy audio synthesis.
- 🚌 **Memory bus**: Centralized address mapping and I/O routing.
- ⏱️ **Timers**: DIV, TIMA, TMA and TAC register handling.
- 💾 **Cartridges**: ROM parsing, memory banking and cartridge RAM.
- 🎛️ **Input**: Joypad state and button register handling.
- 🔌 **Hardware interfaces**: DMA, serial communication and interrupts.

> [!NOTE]
> This project is an emulation implementation, not a replacement for original hardware. Compatibility depends on the accuracy and completeness of individual hardware subsystems.
> 
> For exact hardware specifications and register behaviors, refer to the official [Pan Docs Documentation](https://gbdev.io/pandocs/).

---

## ⚙️ Core Components

| Component | Directory | Responsibility |
|:--|:--|:--|
| **CPU** | `CPU/` | LR35902 instruction decoding, registers, arithmetic, control flow and interrupts |
| **PPU** | `GPU/` | LCD modes, background, window, sprites and framebuffer rendering |
| **APU** | `Audio/` | Square, wave and noise audio channels |
| **Bus & I/O** | `Communication/` | Memory access, address routing, DMA and serial registers |
| **Memory** | `Memory/` | ROM, BIOS, WRAM, VRAM, OAM, HRAM and cartridge management |
| **Input** | `Controls/` | Joypad buttons and input register |
| **Timers** | `Timers/` | Hardware divider, timer counters and timer interrupts |
| **Interrupts** | `CPU/` | Interrupt requests, enable flags and interrupt handling |

---

## 🚌 Architecture

The `BUS` class is the central interface for CPU memory accesses. It routes reads and writes to the appropriate memory region or hardware component.

### Memory Map

| Address Range | Region | Description |
|:--|:--|:--|
| `0x0000–0x3FFF` | ROM Bank 0 | Fixed cartridge ROM bank, or boot ROM mapping where applicable |
| `0x4000–0x7FFF` | ROM Bank | Switchable cartridge ROM bank |
| `0x8000–0x9FFF` | VRAM | Video memory |
| `0xA000–0xBFFF` | External RAM | Cartridge RAM or mapper-controlled registers |
| `0xC000–0xDFFF` | WRAM | Work RAM |
| `0xE000–0xFDFF` | Echo RAM | Mirror of the applicable WRAM region |
| `0xFE00–0xFE9F` | OAM | Sprite attributes |
| `0xFEA0–0xFEFF` | Unusable | Unusable memory area |
| `0xFF00–0xFF7F` | I/O Registers | Hardware control and status registers |
| `0xFF80–0xFFFE` | HRAM | High RAM |
| `0xFFFF` | IE | Interrupt Enable register |

### I/O Routing

The `IO` subsystem dispatches hardware register accesses to the appropriate components.

| Address | Hardware | Responsibility |
|:--|:--|:--|
| `0xFF00` | Joypad | Button selection and input state |
| `0xFF01–0xFF02` | Serial Port | Serial transfer registers |
| `0xFF04–0xFF07` | Timer | Divider and programmable timer |
| `0xFF0F` | Interrupt Controller | Interrupt request flags |
| `0xFF10–0xFF26` | APU | Audio channel and sound control registers |
| `0xFF30–0xFF3F` | Wave RAM | Custom waveform samples |
| `0xFF40–0xFF4B` | PPU | LCD control, status, scrolling and palettes |
| `0xFF46` | DMA | OAM DMA transfer trigger |
| `0xFF4D–0xFF70` | I/O Registers | Additional hardware registers, depending on address |

**Execution Flow**
1. The CPU fetches and executes instructions.
2. Memory accesses are routed through `BUS`.
3. The bus and its associated timing logic advance hardware components.
4. The PPU, APU, timers and other peripherals update according to their implemented timing behavior.

> [!IMPORTANT]
> The memory map above describes the Game Boy address space. Exact access restrictions, timing behavior and register side effects depend on the implementation of each subsystem.

---

## 💾 MBC Support

Memory Bank Controllers allow cartridges to access ROM banks and, where supported, external RAM and additional hardware.

| Controller | Implementation | Notes |
|:--|:--|:--|
| No MBC | `NoMBC.cs` | Standard ROM cartridges |
| MBC1 | `MBC1.cs` | ROM/RAM banking |
| MBC2 | `MBC2.cs` | Banking and internal cartridge RAM |
| MBC3 | `MBC3.cs` | Banking and RTC support |
| MBC5 | `MBC5.cs` | Extended ROM/RAM banking |
| MBC6 | `MBC6.cs` | Specialized cartridge controller |
| MBC7 | `MBC7.cs` | Specialized controller |
| Camera | `MBCCamera.cs` | Game Boy Camera cartridge interface |

---

## 📂 Project Structure

<details>
<summary><b>Expand the source tree</b></summary>

```text
GB/
├── Audio/
│   ├── APU.cs
│   ├── AudioBuffer.cs
│   ├── Channel.cs
│   ├── EnvelopeChannel.cs
│   ├── NoiseChannel.cs
│   ├── SquareChannel1.cs
│   ├── SquareChannel2.cs
│   └── WaveChannel.cs
├── Communication/
│   ├── BUS.cs
│   ├── DMA.cs
│   ├── IO.cs
│   └── SerialPort.cs
├── Controls/
│   ├── Button.cs
│   └── Joypad.cs
├── CPU/
│   ├── Instructions/
│   │   ├── Instructions.Alu.cs
│   │   ├── Instructions.CB.cs
│   │   ├── Instructions.JC.cs
│   │   ├── Instructions.Ld.cs
│   │   └── Instructions.cs
│   ├── CPU.cs
│   ├── Interrupt.cs
│   ├── InterruptController.cs
│   └── Registers.cs
├── GPU/
│   ├── PPU.cs
│   ├── PPU.Background.cs
│   ├── PPU.Rendering.cs
│   ├── PPU.Sprites.cs
│   ├── Screen.cs
│   └── SpriteInfo.cs
├── Interfaces/
│   └── IRWInterface.cs
├── Memory/
│   ├── MBC/
│   │   ├── MBC.cs
│   │   ├── MBC1.cs
│   │   ├── MBC2.cs
│   │   ├── MBC3.cs
│   │   ├── MBC5.cs
│   │   ├── MBC6.cs
│   │   ├── MBC7.cs
│   │   ├── MBCCamera.cs
│   │   ├── MBCManager.cs
│   │   ├── NoMBC.cs
│   │   └── RTC.cs
│   ├── BIOS.cs
│   ├── Cartridge.cs
│   ├── HRAM.cs
│   ├── IORegisters.cs
│   ├── Memory.cs
│   ├── OAM.cs
│   ├── VRAM.cs
│   ├── WaveRAM.cs
│   └── WRAM.cs
├── Timers/
│   └── Timer.cs
├── Utils/
│   └── BitUtils.cs
├── GameBoy.cs
└── GB.csproj

```

---

## 🚀 Getting Started

### Requirements

* [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
* A compatible frontend or a custom application referencing the core library

### Build

Run the following commands from the repository root:

```bash
dotnet restore GB/GB.csproj
dotnet build GB/GB.csproj --configuration Release
```

### Use as a Project Dependency

Add a project reference to your frontend:

```xml
<ItemGroup>
  <ProjectReference Include="../GB/GB.csproj"/>
</ItemGroup>
```

---

## 📚 Resources

* [Pan Docs](https://gbdev.io/pandocs/) – Game Boy hardware documentation
* [Game Boy CPU Manual](https://rgbds.gbdev.io/docs/gbz80.7) – CPU instruction reference

---