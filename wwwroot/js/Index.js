document.addEventListener('DOMContentLoaded', function () {
    // botones de recolección
    const btnRecolectarMadera = document.getElementById('btnRecolectarMadera');
    const btnRecolectarPiedra = document.getElementById('btnRecolectarPiedra');
    const btnRecolectarComida = document.getElementById('btnRecolectarComida');

    // navbar y modal de nombre
    const changeNameBtn = document.getElementById('changeNameBtn');
    const changeNameModal = new bootstrap.Modal(document.getElementById('changeNameModal'));
    const nombreInput = document.getElementById('nombre');
    const acceptNameChangeBtn = document.getElementById('acceptNameChange');
    const playerNameDisplay = document.getElementById('playerNameDisplay');

    // contadores en cards
    const maderaActual = document.getElementById('maderaActual');
    const piedraActual = document.getElementById('piedraActual');
    const comidaActual = document.getElementById('comidaActual');

    // tiempo partida
    const tiempoPanel = document.getElementById('tiempoPartidaCompleta');
    const tiempoValor = document.getElementById('tiempoPartidaValor');

    // partida
    const partidaId = parseInt(document.getElementById('partidaId').value, 10);

    // aca va toda la parte del nombre del jugador 
    changeNameBtn.addEventListener('click', function () {
        nombreInput.value = '';
        changeNameModal.show();
        setTimeout(() => nombreInput.focus(), 300);
    });

    acceptNameChangeBtn.addEventListener('click', async () => {
        const nombre = (nombreInput.value || '').trim();
        if (!nombre) { nombreInput.classList.add('is-invalid'); return; }

        const formdata = new FormData();
        formdata.append('nombre', nombre);

        const res = await fetch('/Jugador/GuardarJugador', { method: 'POST', body: formdata });
        if (!res.ok) { nombreInput.classList.add('is-invalid'); return; }

        const data = await res.json();
        if (data.ok) {
            playerNameDisplay.innerHTML = `Jugador: <em>${data.nombre}</em>`;
            nombreInput.classList.remove('is-invalid');
            changeNameModal.hide();
        }
    });

    //aca va la parte de recolección
    async function Registro(tipo) {
        const fd = new FormData();
        fd.append('tipo', String(tipo));       // aca se pasa el tipo de recurso que como es un enum se pasa como 0 1 o 2
        fd.append('partidaId', String(partidaId));

        const resp = await fetch('/Registro/GuardarRegistro', { method: 'POST', body: fd });
        if (!resp.ok) {
            const err = await resp.text();
            alert('Error en la solicitud: ' + err);
            return;
        }

        const data = await resp.json();
        if (!data.ok) return;

        // actualizamos los contadores
        maderaActual.textContent = data.totales.madera;
        piedraActual.textContent = data.totales.piedra;
        comidaActual.textContent = data.totales.comida;

        // si individualmente llegan a la meta se deshabilita el boton
        if (data.metasAlcanzadas?.madera) btnRecolectarMadera.disabled = true;
        if (data.metasAlcanzadas?.piedra) btnRecolectarPiedra.disabled = true;
        if (data.metasAlcanzadas?.comida) btnRecolectarComida.disabled = true;

        // si todas las metas se completas van a estae desabilitados todos los botones y se muestra el tiempo
        if (data.completada) {
            btnRecolectarMadera.disabled = true;
            btnRecolectarPiedra.disabled = true;
            btnRecolectarComida.disabled = true;

            // tiempo de partida en mm:ss
            if (data.tiempoPartida) {
                tiempoValor.textContent = data.tiempoPartida;
                tiempoPanel.classList.remove('d-none');
            }
        }
    }

    // eventlistener de los botones de recursos
    btnRecolectarMadera.addEventListener('click', () => Registro(0));
    btnRecolectarPiedra.addEventListener('click', () => Registro(1));
    btnRecolectarComida.addEventListener('click', () => Registro(2));
});