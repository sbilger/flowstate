// Focus-mode SignalR client. Connects to the server-authoritative timer hub and pushes
// ticks back into the Blazor component, which renders the visual countdown ring.
window.flowstateFocus = (function () {
  let connection = null;
  let dotNetRef = null;

  async function start(sessionId, ref) {
    dotNetRef = ref;

    // Load the SignalR client from CDN if not already present.
    if (!window.signalR) {
      await new Promise((resolve, reject) => {
        const s = document.createElement('script');
        s.src = 'https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/8.0.0/signalr.min.js';
        s.onload = resolve;
        s.onerror = reject;
        document.head.appendChild(s);
      });
    }

    connection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/focus')
      .withAutomaticReconnect()
      .build();

    connection.on('tick', (tick) => {
      if (dotNetRef) {
        dotNetRef.invokeMethodAsync('OnTick', tick.elapsedSeconds, tick.plannedSeconds, tick.overtime);
      }
    });

    await connection.start();
    await connection.invoke('JoinSession', sessionId);
  }

  async function stop(sessionId) {
    try {
      if (connection) {
        await connection.invoke('LeaveSession', sessionId);
        await connection.stop();
      }
    } catch (e) { /* ignore */ }
    connection = null;
    dotNetRef = null;
  }

  return { start, stop };
})();
