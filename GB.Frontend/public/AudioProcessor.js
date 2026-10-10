class AudioProcessor extends AudioWorkletProcessor {

    constructor() {
        super();
        this.buffer = new Float32Array();
        this.position = 0;
        this.requestPending = false;

        this.port.onmessage = e => {
            const data = e.data;
            if (!(data instanceof Float32Array)) 
                return;

            const remaining = this.buffer.subarray(this.position);
            this.buffer = new Float32Array(remaining.length + data.length);
            this.buffer.set(remaining);
            this.buffer.set(data, remaining.length);
            this.position = 0;
            this.requestPending = false;
        };
    }

    process(_, outputs) {
        const [l, r] = outputs[0];

        for (let i = 0; i < l.length; i++) {
            l[i] = this.buffer[this.position++] ?? 0;
            r[i] = this.buffer[this.position++] ?? 0;
        }

        if (this.buffer.length - this.position < 1024 && !this.requestPending) {
            this.requestPending = true;
            this.port.postMessage({type: "requestData"});
        }

        return true;
    }
}

registerProcessor("audio", AudioProcessor);