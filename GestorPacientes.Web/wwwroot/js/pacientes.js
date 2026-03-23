        let paginaActual = 1;
        let terminoBusqueda = '';
        let timeoutBusqueda;
        let modoEdicion = false;
        let pacienteEditandoId = null;
        let pacienteDesactivandoId = null;

        document.addEventListener('DOMContentLoaded', () => cargarPacientes());

        function buscar() {
            clearTimeout(timeoutBusqueda);
            timeoutBusqueda = setTimeout(() => {
                terminoBusqueda = document.getElementById('inputBusqueda').value.trim();
                paginaActual = 1;
                document.getElementById('textoExportar').textContent =
                    terminoBusqueda ? `Exportar "${terminoBusqueda}"` : 'Exportar Excel';
                cargarPacientes();
            }, 400);
        }

        async function cargarPacientes() {
            const tbody = document.getElementById('tablaPacientes');
            tbody.innerHTML = `<tr><td colspan="5" class="text-center py-8 text-gray-400">
                <div class="flex items-center justify-center gap-2">Cargando...</div></td></tr>`;

            const params = new URLSearchParams({
                pagina: paginaActual,
                recordsPorPagina: 4,
                ...(terminoBusqueda && { termino: terminoBusqueda })
            });

            const response = await fetch(`/Pacientes/ListarPaginado?${params}`);
            const data = await response.json();

            renderTabla(data.data);
            renderPaginacion(data.totalPaginas, data.paginaActual, data.totalRegistros);
        }

        function renderTabla(pacientes) {
            const tbody = document.getElementById('tablaPacientes');

            if (!pacientes.length) {
                tbody.innerHTML = `<tr><td colspan="5" class="text-center py-8 text-gray-400">
                    No se encontraron pacientes</td></tr>`;
                return;
            }

            tbody.innerHTML = pacientes.map(p => `
                <tr class="hover:bg-gray-50 transition">
                    <td class="px-6 py-4 text-gray-500 text-sm tracking-wide">${p.documento}</td>
                    <td class="px-6 py-4 font-medium text-gray-800">${p.nombre}</td>
                    <td class="px-6 py-4 text-gray-600">${p.edad} años</td>
                    <td class="px-6 py-4 text-gray-600">${p.email}</td>
                    <td class="px-6 py-4">
                        <div class="flex items-center gap-2">
                            <button onclick="abrirModalEditar(${p.id})"
                                class="p-1.5 text-gray-400 hover:text-emerald-600 hover:bg-emerald-50 rounded-lg transition"
                                title="Editar">
                                <svg xmlns="http://www.w3.org/2000/svg" width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
                            </button>
                            <button onclick="abrirModalConfirmar(${p.id}, '${p.nombre}')"
                                class="p-1.5 text-gray-400 hover:text-red-500 hover:bg-red-50 rounded-lg transition"
                                title="Desactivar">
                                <svg xmlns="http://www.w3.org/2000/svg" width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="3 6 5 6 21 6"/><path d="M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6"/><path d="M10 11v6"/><path d="M14 11v6"/><path d="M9 6V4a1 1 0 0 1 1-1h4a1 1 0 0 1 1 1v2"/></svg>
                            </button>
                        </div>
                    </td>
                </tr>`).join('');
        }

        function renderPaginacion(totalPaginas, paginaActual, totalRegistros) {
            document.getElementById('infoPaginacion').textContent =
                `${totalRegistros} paciente(s) encontrado(s)`;

            const contenedor = document.getElementById('controlesPaginacion');
            if (totalPaginas <= 1) { contenedor.innerHTML = ''; return; }

            let html = '';
            html += `<button onclick="cambiarPagina(${paginaActual - 1})" ${paginaActual === 1 ? 'disabled' : ''}
                class="px-3 py-1 text-xs rounded-lg border border-gray-200 text-gray-600 hover:bg-gray-50 disabled:opacity-40 disabled:cursor-not-allowed">&#8249;</button>`;

            for (let i = 1; i <= totalPaginas; i++) {
                html += `<button onclick="cambiarPagina(${i})"
                    class="px-3 py-1 text-xs rounded-lg border ${i === paginaActual
                        ? 'bg-emerald-600 text-white border-emerald-600'
                        : 'border-gray-200 text-gray-600 hover:bg-gray-50'}">
                    ${i}</button>`;
            }

            html += `<button onclick="cambiarPagina(${paginaActual + 1})" ${paginaActual === totalPaginas ? 'disabled' : ''}
                class="px-3 py-1 text-xs rounded-lg border border-gray-200 text-gray-600 hover:bg-gray-50 disabled:opacity-40 disabled:cursor-not-allowed">&#8250;</button>`;

            contenedor.innerHTML = html;
        }

        function cambiarPagina(pagina) {
            paginaActual = pagina;
            cargarPacientes();
        }

        
        function abrirModalCrear() {
            modoEdicion = false;
            pacienteEditandoId = null;
            document.getElementById('modalTitulo').textContent = 'Nuevo Paciente';
            const inputDoc = document.getElementById('numeroDocumento');
            inputDoc.disabled = false;
            inputDoc.classList.remove('bg-gray-100', 'text-gray-400', 'cursor-not-allowed');
            limpiarModal();
            document.getElementById('modal').classList.replace('hidden', 'flex');
        }

        
        async function abrirModalEditar(id) {
            modoEdicion = true;
            pacienteEditandoId = id;
            document.getElementById('modalTitulo').textContent = 'Editar Paciente';

            const inputDoc = document.getElementById('numeroDocumento');
            inputDoc.disabled = true;
            inputDoc.classList.add('bg-gray-100', 'text-gray-400', 'cursor-not-allowed');

            limpiarModal();
            document.getElementById('modal').classList.replace('hidden', 'flex');

            const response = await fetch(`/Pacientes/ObtenerPorId?id=${id}`);
            const p = await response.json();

            inputDoc.value = p.documento;
            document.getElementById('nombreCompleto').value = p.nombreCompleto;
            document.getElementById('email').value = p.email;
            document.getElementById('fechaNacimiento').value = p.fechaNacimiento.split('T')[0];
        }

        function cerrarModal() {
            document.getElementById('modal').classList.replace('flex', 'hidden');
            const inputDoc = document.getElementById('numeroDocumento');
            inputDoc.disabled = false;
            inputDoc.classList.remove('bg-gray-100', 'text-gray-400', 'cursor-not-allowed');
            limpiarModal();
        }

        function limpiarModal() {
            ['numeroDocumento', 'nombreCompleto', 'fechaNacimiento', 'email'].forEach(id => {
                document.getElementById(id).value = '';
            });
            ['errorDocumento', 'errorNombre', 'errorFecha', 'errorEmail'].forEach(id => {
                const el = document.getElementById(id);
                el.textContent = '';
                el.classList.add('hidden');
            });
        }

        function limpiarErrores() {
            ['errorDocumento', 'errorNombre', 'errorFecha', 'errorEmail'].forEach(id => {
                const el = document.getElementById(id);
                el.textContent = '';
                el.classList.add('hidden');
            });
        }

        
        async function guardar() {
            if (modoEdicion) {
                await actualizar();
            } else {
                await crear();
            }
        }

        async function crear() {

            limpiarErrores()

            const documento = document.getElementById('numeroDocumento').value.trim();
            const nombre = document.getElementById('nombreCompleto').value.trim();
            const fecha = document.getElementById('fechaNacimiento').value;
            const email = document.getElementById('email').value.trim();

            let hayErrores = false;
            if (!documento) { mostrarError('errorDocumento', 'El documento es obligatorio.'); hayErrores = true; }
            if (!nombre) { mostrarError('errorNombre', 'El nombre es obligatorio.'); hayErrores = true; }
            if (!fecha) { mostrarError('errorFecha', 'La fecha de nacimiento es obligatoria.'); hayErrores = true; }
            if (!email) { mostrarError('errorEmail', 'El email es obligatorio.'); hayErrores = true; }
            if (hayErrores) return;

            const dto = { numeroDocumento: documento, nombreCompleto: nombre, fechaNacimiento: fecha, email };

            const response = await fetch('/Pacientes/Crear', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(dto)
            });

            if (response.ok) {
                const paciente = await response.json();
                cerrarModal();
                cargarPacientes();
                mostrarToast(`Paciente ${paciente.nombre} con ID ${paciente.documento} registrado exitosamente`, 'exito');
            } else {
                const errores = await response.json();
                mostrarErrores(errores);
            }
        }

        async function actualizar() {
            const nombre = document.getElementById('nombreCompleto').value.trim();
            const fecha = document.getElementById('fechaNacimiento').value;
            const email = document.getElementById('email').value.trim();

            let hayErrores = false;
            if (!nombre) { mostrarError('errorNombre', 'El nombre es obligatorio.'); hayErrores = true; }
            if (!fecha) { mostrarError('errorFecha', 'La fecha de nacimiento es obligatoria.'); hayErrores = true; }
            if (!email) { mostrarError('errorEmail', 'El email es obligatorio.'); hayErrores = true; }
            if (hayErrores) return;

            const dto = { nombreCompleto: nombre, fechaNacimiento: fecha, email };

            const response = await fetch(`/Pacientes/Actualizar?id=${pacienteEditandoId}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(dto)
            });

            if (response.ok) {
                cerrarModal();
                cargarPacientes();
                mostrarToast('Paciente actualizado exitosamente', 'exito');
            } else {
                const errores = await response.json();
                mostrarErrores(errores);
            }
        }

        
        function abrirModalConfirmar(id, nombre) {
            pacienteDesactivandoId = id;
            document.getElementById('nombreConfirmar').textContent = nombre;
            document.getElementById('modalConfirmar').classList.replace('hidden', 'flex');
        }

        function cerrarModalConfirmar() {
            pacienteDesactivandoId = null;
            document.getElementById('modalConfirmar').classList.replace('flex', 'hidden');
        }

        async function confirmarDesactivar() {
            const response = await fetch(`/Pacientes/Desactivar?id=${pacienteDesactivandoId}`, {
                method: 'PUT'
            });

            if (response.ok) {
                cerrarModalConfirmar();
                cargarPacientes();
                mostrarToast('Paciente desactivado exitosamente', 'exito');
            } else {
                const error = await response.json();
                mostrarToast(error.mensaje, 'error');
            }
        }

        function mostrarErrores(errores) {
            if (Array.isArray(errores)) {
                errores.forEach(e => {
                    if (e.toLowerCase().includes('documento')) mostrarError('errorDocumento', e);
                    else if (e.toLowerCase().includes('nombre')) mostrarError('errorNombre', e);
                    else if (e.toLowerCase().includes('fecha')) mostrarError('errorFecha', e);
                    else if (e.toLowerCase().includes('email')) mostrarError('errorEmail', e);
                });
            } else if (errores.mensaje) {
                mostrarToast(errores.mensaje, 'error');
            }
        }

        function mostrarError(id, mensaje) {
            const el = document.getElementById(id);
            el.textContent = mensaje;
            el.classList.remove('hidden');
        }

        function exportarExcel() {
            const params = new URLSearchParams();
            if (terminoBusqueda) params.append('termino', terminoBusqueda);
            window.location.href = `/Pacientes/ExportarExcel?${params}`;
        }

        function mostrarToast(mensaje, tipo) {
            const toast = document.getElementById('toast');
            const contenido = document.getElementById('toastContenido');
            document.getElementById('toastMensaje').textContent = mensaje;

            contenido.className = tipo === 'exito'
                ? 'flex items-center gap-3 px-4 py-3 rounded-lg shadow-lg text-sm font-medium text-white bg-emerald-600'
                : 'flex items-center gap-3 px-4 py-3 rounded-lg shadow-lg text-sm font-medium text-white bg-red-500';

            toast.classList.remove('hidden');
            setTimeout(() => toast.classList.add('hidden'), 3500);
        }
