document.addEventListener('DOMContentLoaded', function () {
    const mainContainer = document.getElementById('mainContainer');
    const partidaId = parseInt(mainContainer.dataset.partidaId, 10);

    const btnRecolectarMadera = document.getElementById('btnRecolectarMadera');
    const btnRecolectarPiedra = document.getElementById('btnRecolectarPiedra');
    const btnRecolectarComida = document.getElementById('btnRecolectarComida');

    const changeNameBtn = document.getElementById('changeNameBtn');
    const changeNameModal = new bootstrap.Modal(document.getElementById('changeNameModal'));
    const nombreInput = document.getElementById('nombre');
    const acceptNameChangeBtn = document.getElementById('acceptNameChange');
    const playerNameDisplay = document.getElementById('playerNameDisplay');

    const maderaActual = document.getElementById('maderaActual');
    const piedraActual = document.getElementById('piedraActual');
    const comidaActual = document.getElementById('comidaActual');

    const tiempoPanel = document.getElementById('tiempoPartidaCompleta');
    const tiempoValor = document.getElementById('tiempoPartidaValor');

    const tablaResultados = document.getElementById('tablaResultados');
    const cuerpoResultados = document.getElementById('tablaResultadosBody');

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

    async function Registro(tipo) {
        const fd = new FormData();
        fd.append('tipo', String(tipo));
        fd.append('partidaId', String(partidaId));

        const resp = await fetch('/Registro/GuardarRegistro', { method: 'POST', body: fd });
        if (!resp.ok) { alert('Error en la solicitud'); return; }

        const data = await resp.json();
        if (!data.ok) return;

        maderaActual.textContent = data.totales.madera;
        piedraActual.textContent = data.totales.piedra;
        comidaActual.textContent = data.totales.comida;

        if (data.metasAlcanzadas?.madera) btnRecolectarMadera.disabled = true;
        if (data.metasAlcanzadas?.piedra) btnRecolectarPiedra.disabled = true;
        if (data.metasAlcanzadas?.comida) btnRecolectarComida.disabled = true;

        if (data.partidaCompletada) {

            btnRecolectarMadera.disabled = true;
            btnRecolectarPiedra.disabled = true;
            btnRecolectarComida.disabled = true;

            if (data.tiempoPartida) {
                tiempoValor.textContent = data.tiempoPartida;
                tiempoPanel.classList.remove('d-none');
            }

            if (data.registros && Array.isArray(data.registros)) {
                cuerpoResultados.innerHTML = '';
                data.registros.forEach(j => {
                    const fila = document.createElement('tr');
                    fila.innerHTML = `
            <td>${j.nombreJugador || 'Jugador ' + j.jugadorId}</td>
            <td>${j.sumaTotalMadera}</td>
            <td>${j.sumaTotalPiedra}</td>
            <td>${j.sumaTotalComida}</td>
            <td>${j.sumaTotalRecursos}</td>
          `;
                    cuerpoResultados.appendChild(fila);
                });
                tablaResultados.classList.remove('d-none');
            }
        }
    }

    btnRecolectarMadera.addEventListener('click', () => Registro(0));
    btnRecolectarPiedra.addEventListener('click', () => Registro(1));
    btnRecolectarComida.addEventListener('click', () => Registro(2));
});
