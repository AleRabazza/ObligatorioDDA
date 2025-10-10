document.addEventListener('DOMContentLoaded', function () {

    const contenedorPrincipal = document.getElementById('mainContainer');
    const idPartida = parseInt(contenedorPrincipal.dataset.partidaId, 10);

    const requiereNombre = (contenedorPrincipal.dataset.requiereNombre === 'True' || contenedorPrincipal.dataset.requiereNombre === 'true');

    const botonRecolectarMadera = document.getElementById('btnRecolectarMadera');
    const botonRecolectarPiedra = document.getElementById('btnRecolectarPiedra');
    const botonRecolectarComida = document.getElementById('btnRecolectarComida');
    const botonReiniciar = document.getElementById('btnReiniciar');

    const botonCambiarNombre = document.getElementById('changeNameBtn');
    const modalCambiarNombre = new bootstrap.Modal(document.getElementById('changeNameModal'));
    const inputNombre = document.getElementById('nombre');
    const botonAceptarNombre = document.getElementById('acceptNameChange');
    const etiquetaNombreJugador = document.getElementById('playerNameDisplay');

    const maderaActual = document.getElementById('maderaActual');
    const piedraActual = document.getElementById('piedraActual');
    const comidaActual = document.getElementById('comidaActual');

    const panelTiempo = document.getElementById('tiempoPartidaCompleta');
    const valorTiempo = document.getElementById('tiempoPartidaValor');

    const tablaResultados = document.getElementById('tablaResultados');
    const cuerpoResultados = document.getElementById('tablaResultadosBody');


    const modalMinijuegoElemento = document.getElementById('miniGameModal');
    const modalMinijuego = new bootstrap.Modal(modalMinijuegoElemento);
    const etiquetaCuentaRegresiva = document.getElementById('miniGameCountdown');
    const etiquetaEnunciado = document.getElementById('miniGameEnunciado');
    const etiquetaPregunta = document.getElementById('miniGamePregunta');
    const areaDinamica = document.getElementById('miniGameDynamicArea');
    const botonEnviarMinijuego = document.getElementById('miniGameSubmit');
    const botonCancelarMinijuego = document.getElementById('miniGameCancel');
    const botonCerrarXMinijuego = document.getElementById('miniGameCloseX');
    const etiquetaSecuencia = document.getElementById('miniGameSecuencia');



    let tipoActual = null;           
    let idTemporizador = null;
    let segundosRestantes = 60;

    if (requiereNombre) {

        inputNombre.value = '';
        modalCambiarNombre.show();
        setTimeout(() => inputNombre.focus(), 250);
    }

    botonCambiarNombre.addEventListener('click', function () {
        inputNombre.value = '';
        modalCambiarNombre.show();
        setTimeout(() => inputNombre.focus(), 300);
    });

    botonAceptarNombre.addEventListener('click', async () => {
        const nombre = (inputNombre.value || '').trim();
        if (!nombre) { inputNombre.classList.add('is-invalid'); return; }

        const formdata = new FormData();
        formdata.append('nombre', nombre);

        const res = await fetch('/Jugador/GuardarJugador', { method: 'POST', body: formdata });
        if (!res.ok) { inputNombre.classList.add('is-invalid'); return; }

        const data = await res.json();
        if (data.ok) {
            etiquetaNombreJugador.innerHTML = `Jugador: <em>${data.nombre}</em>`;
            inputNombre.classList.remove('is-invalid');
            modalCambiarNombre.hide();
        }
    });

    function iniciarCuentaRegresiva(segundos) {
        clearInterval(idTemporizador);
        segundosRestantes = segundos;
        etiquetaCuentaRegresiva.textContent = segundosRestantes.toString();

        idTemporizador = setInterval(function () {
            segundosRestantes--;
            etiquetaCuentaRegresiva.textContent = segundosRestantes.toString();

            if (segundosRestantes <= 0) {
                clearInterval(idTemporizador);
                modalMinijuego.hide();
                alert("Tiempo agotado");
            }
        }, 1000);
    }

    function renderizarEntradasPara(tipo) {

        if (tipo === 0) {
            areaDinamica.innerHTML = `
                <label class="form-label text-white">Respuesta</label>
                <input type="number" min="0" max="999" step="1" class="form-control" id="miniGameAnswer" placeholder="Ej: 135">
            `;
        } else if (tipo === 1) {
            areaDinamica.innerHTML = `
                <div class="d-flex gap-2">
                    <button type="button" class="btn btn-outline-light" data-val="Sí">Sí</button>
                    <button type="button" class="btn btn-outline-light" data-val="No">No</button>
                </div>
                <input type="hidden" id="miniGameAnswer">
            `;
            areaDinamica.querySelectorAll('button[data-val]').forEach(function (boton) {
                boton.addEventListener('click', function () {
                    areaDinamica.querySelector('#miniGameAnswer').value = boton.getAttribute('data-val');
                });
            });
        } else {
            areaDinamica.innerHTML = `
                <div class="d-flex gap-2">
                    <button type="button" class="btn btn-outline-light" data-val="Verdadero">Verdadero</button>
                    <button type="button" class="btn btn-outline-light" data-val="Falso">Falso</button>
                </div>
                <input type="hidden" id="miniGameAnswer">
            `;
            areaDinamica.querySelectorAll('button[data-val]').forEach(function (boton) {
                boton.addEventListener('click', function () {
                    areaDinamica.querySelector('#miniGameAnswer').value = boton.getAttribute('data-val');
                });
            });
        }
    }

    async function abrirMinijuego(tipo) {
        tipoActual = tipo;
        areaDinamica.innerHTML = "";
        etiquetaEnunciado.textContent = "Cargando...";
        etiquetaPregunta.textContent = "";
        botonEnviarMinijuego.disabled = true;

        try {
            const respuesta = await fetch(`/Minijuego/Generar?partidaId=${idPartida}&tipo=${tipo}`);
            const datos = await respuesta.json();

            if (!datos.ok) { alert(datos.msg || "Error"); return; }

            etiquetaEnunciado.textContent = datos.enunciado || "";
            etiquetaPregunta.textContent = "";                // limpiamos por si acaso
            areaDinamica.innerHTML = "";
            botonEnviarMinijuego.disabled = true;

            modalMinijuego.show();
            iniciarCuentaRegresiva(datos.countdown || 60);

            if (tipo === 1 && datos.datos) {

                const secuencia = datos.datos.secuencia || datos.datos.Secuencia || [];

                reproducirSecuencia(secuencia, 1000, 500, function () {
                    etiquetaPregunta.textContent = datos.preguntaDiferida || "Preparándose pregunta...";
                    renderizarEntradasPara(1);               
                    botonEnviarMinijuego.disabled = false;    
                });
            } else {

                etiquetaPregunta.textContent = datos.pregunta || "";
                renderizarEntradasPara(tipo);
                botonEnviarMinijuego.disabled = false;
            }
        } catch {
            alert("Error al cargar minijuego");
        }
    }

    function reproducirSecuencia(numeros, mostrarMs, pausaMs, alTerminar) {
        let indice = 0;

        function mostrarSiguiente() {
            if (indice >= numeros.length) {
                etiquetaSecuencia.textContent = "";
                if (typeof alTerminar === "function") { alTerminar(); }
                return;
            }

            etiquetaSecuencia.textContent = String(numeros[indice]);
            indice++;

            setTimeout(function () {
                etiquetaSecuencia.textContent = "";
                setTimeout(mostrarSiguiente, pausaMs);
            }, mostrarMs);
        }

        mostrarSiguiente();
    }


    async function enviarRespuestaMinijuego() {
        const inputOculto = areaDinamica.querySelector('#miniGameAnswer');
        let respuestaUsuario = "";

        if (inputOculto) {
            respuestaUsuario = (inputOculto.value || "").trim();
        } else {
            const inputNumero = areaDinamica.querySelector('input[type=number]');
            respuestaUsuario = inputNumero ? (inputNumero.value || "").trim() : "";
        }

        if (!respuestaUsuario) {
            alert("Debes responder para continuar.");
            return;
        }

        const datosFormulario = new FormData();
        datosFormulario.append("partidaId", String(idPartida));
        datosFormulario.append("tipo", String(tipoActual));
        datosFormulario.append("respuesta", respuestaUsuario);

        try {
            const respuesta = await fetch("/Minijuego/Responder", { method: "POST", body: datosFormulario });
            const data = await respuesta.json();

            clearInterval(idTemporizador);
            modalMinijuego.hide();

            if (!data.ok) {
                alert(data.msg || "Error");
                return;
            }

            alert(data.msg || (data.correcto ? "¡Correcto!" : "Incorrecto"));

            // Actualizar contadores de recursos
            if (data.correcto && data.totales) {
                maderaActual.textContent = data.totales.madera;
                piedraActual.textContent = data.totales.piedra;
                comidaActual.textContent = data.totales.comida;

                if (data.metasAlcanzadas && data.metasAlcanzadas.madera) botonRecolectarMadera.disabled = true;
                if (data.metasAlcanzadas && data.metasAlcanzadas.piedra) botonRecolectarPiedra.disabled = true;
                if (data.metasAlcanzadas && data.metasAlcanzadas.comida) botonRecolectarComida.disabled = true;
            }

            // si la partida termina muestro resultados
            if (data.partidaCompletada) {
                if (data.tiempoPartida) {
                    valorTiempo.textContent = data.tiempoPartida;
                    panelTiempo.classList.remove('d-none');
                }

                if (data.registros && Array.isArray(data.registros)) {
                    cuerpoResultados.innerHTML = '';
                    data.registros.forEach(function (j) {
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

           
                botonRecolectarMadera.disabled = true;
                botonRecolectarPiedra.disabled = true;
                botonRecolectarComida.disabled = true;
            }
        } catch {
            alert("Error al enviar respuesta");
        }
    }

    function cerrarMinijuego() {
        clearInterval(idTemporizador);
        modalMinijuego.hide();
    }


    botonRecolectarMadera.addEventListener('click', () => abrirMinijuego(0));
    botonRecolectarPiedra.addEventListener('click', () => abrirMinijuego(1));
    botonRecolectarComida.addEventListener('click', () => abrirMinijuego(2));

    // modal de minijuego
    botonEnviarMinijuego.addEventListener('click', enviarRespuestaMinijuego);
    botonCancelarMinijuego.addEventListener('click', cerrarMinijuego);
    botonCerrarXMinijuego.addEventListener('click', cerrarMinijuego);

    // reiniciar partida
    botonReiniciar.addEventListener('click', async () => {
        const confirmar = confirm("¿Deseas reiniciar la partida?");
        if (!confirmar) return;

        const response = await fetch('/Partida/Reiniciar', { method: 'POST' });
        const data = await response.json();

        if (data.ok) {
            if (data.existente) {
                alert("Ya hay una partida activa creada por otro jugador. Serás redirigido a ella.");
            } else {
                alert("Nueva partida iniciada correctamente.");
            }
            location.reload();
        } else {
            alert("Error al reiniciar la partida: " + data.mensaje);
        }
    });
});