<div align="center">

  <br />

  <h1>🌐 GB.Frontend — Web Client & GUI</h1>

  <p align="center">
    <b>The modern, responsive web interface for the GameBoyOnline emulator project.</b>
    <br />
    <i>Built with React, Vite, and Tailwind CSS to deliver a seamless retro gaming experience in the browser.</i>
  </p>

  <p align="center">
    <a href="#overview"><b>Overview</b></a> •
    <a href="#api-configuration"><b>API Config</b></a> •
    <a href="#project-structure"><b>File Structure</b></a> •
    <a href="#getting-started"><b>Quick Start</b></a>
  </p>

  <p align="center">
    <a href="https://react.dev/">
      <img src="https://img.shields.io/badge/React-20232A?style=for-the-badge&logo=react&logoColor=61DAFB" alt="React" />
    </a>
    <a href="https://vite.dev/">
      <img src="https://img.shields.io/badge/Vite-646CFF?style=for-the-badge&logo=vite&logoColor=white" alt="Vite" />
    </a>
    <a href="https://tailwindcss.com/">
      <img src="https://img.shields.io/badge/Tailwind_CSS-06B6D4?style=for-the-badge&logo=tailwindcss&logoColor=white" alt="Tailwind CSS" />
    </a>
    <img src="https://img.shields.io/badge/JavaScript-F7DF1E?style=for-the-badge&logo=javascript&logoColor=black" alt="JavaScript" />
  </p>
  <br />
</div>

---

<br>
<h2 align="center" id="overview">📖 Overview</h2>
<p align="center">
  <b>GB.Frontend</b> is the user-facing client application of GameBoyOnline. It provides a clean dashboard, game library management, an interactive canvas renderer driven by the WebAssembly core (<b><code>GB.WASM</code></b>), customizable keybindings, and real-time hardware debugging inspectors.
</p>
<br>

---

<br>
<h2 align="center" id="api-configuration">⚙️ API Configuration</h2>
<p align="center">
  The frontend communicates with the backend API to manage ROM hosting, file storage, and user preferences. 
  The base endpoint address is centrally configured inside the <b><code>src/api.js</code></b> file:
</p>
<br>

```javascript
// src/api.js
export const API_URL = "/api";
```

> [!NOTE]
> Modify the `API_URL` variable if your backend API is hosted on a separate port or domain during local development.
<br>

---

<br>
<h2 align="center" id="project-structure">📃 File Structure</h2>

```text
GB.Frontend/
├── public/
│   ├── AudioProcessor.js       # Audio worklet processor for Web Audio API
│   └── wasm/                   # Compiled WebAssembly binaries & interop scripts
├── src/
│   ├── Components/
│   │   ├── BindItem.jsx        # Single keybind configuration component
│   │   ├── Contrast.jsx        # Display contrast settings
│   │   ├── FilesSelect.jsx     # ROM file picker
│   │   ├── Footer.jsx          # Footer component
│   │   ├── Header.jsx          # Header component with status indicator
│   │   ├── HexView.jsx         # Memory hex inspector view
│   │   ├── Interrupts.jsx      # CPU interrupts preview
│   │   ├── Item.jsx            # UI item wrapper
│   │   ├── KeyBinds.jsx        # Keybinds configuration menu
│   │   ├── Pad.jsx             # On-screen joypad
│   │   ├── Registers.jsx       # Live CPU register view
│   │   ├── RomManager.jsx      # ROM management, uploader & manager
│   │   ├── Timers.jsx          # Live timer view
│   │   └── Window.jsx          # Movable window wrapper
│   ├── Utils/
│   │   └── StatusType.jsx      # Status enum helpers
│   ├── api.js                  # API endpoint configuration (API_URL)
│   ├── App.jsx                 # Root application component
│   ├── Emulator.jsx            # Main emulator canvas & core loop controller
│   ├── main.css                # Global styling
│   ├── main.jsx                # React application entry point
│   └── Overlay.jsx             # UI overlays & HUD elements
├── index.html                  # HTML template host
├── package.json                # Project dependencies & scripts
└── vite.config.js              # Vite bundler configuration
```
<br>

---
<br>
<h2 align="center" id="getting-started">⭐ Quick start</h2>

> [!NOTE]
> Ensure you have **Node.js (v25.9.0+)** and **npm (11.17.0+)** installed on your machine.

```bash
cd GB.Frontend
npm install
```

```bash
npm run dev
```

The web client will start locally (typically at `http://localhost:5173`).

```bash
npm run build
```
<br>

---