// Menu lateral: en pantalla grande se contrae a solo iconos; en celular se abre y se cierra
function alternarMenu() {
    const menu = document.getElementById('menuLateral');
    if (window.innerWidth < 768) {
        menu.classList.toggle('abierto');
        return;
    }
    menu.classList.toggle('contraido');
    try { localStorage.setItem('menuContraido', menu.classList.contains('contraido')); } catch (e) { }
}

// Al cargar cada pagina se respeta si el menu estaba contraido
document.addEventListener('DOMContentLoaded', function () {
    try {
        if (localStorage.getItem('menuContraido') === 'true')
            document.getElementById('menuLateral').classList.add('contraido');
    } catch (e) { }
});

// Genera un PDF con el titulo, la grafica (si hay) y las tablas indicadas de la pagina actual.
// opciones = { titulo, subtitulo, archivo, tablas: ['idTabla', ...], grafica: 'idCanvas', horizontal: bool }
function generarPdf(opciones) {
    const { jsPDF } = window.jspdf;
    const doc = new jsPDF({ orientation: opciones.horizontal ? 'landscape' : 'portrait', unit: 'pt', format: 'letter' });
    const margen = 40;
    const ancho = doc.internal.pageSize.getWidth() - margen * 2;

    doc.setFontSize(16);
    doc.text(opciones.titulo, margen, 50);
    doc.setFontSize(10);
    doc.text(opciones.subtitulo || '', margen, 68);
    doc.text('Generado: ' + new Date().toLocaleString('es-GT'), margen, 82);

    let y = 100;

    const lienzo = opciones.grafica ? document.getElementById(opciones.grafica) : null;
    if (lienzo) {
        let w = ancho;
        let h = w * lienzo.height / lienzo.width;
        if (h > 280) {
            w = w * 280 / h;
            h = 280;
        }
        doc.addImage(lienzo.toDataURL('image/png'), 'PNG', margen, y, w, h);
        y += h + 20;
    }

    (opciones.tablas || []).forEach(function (id) {
        const tabla = document.getElementById(id);
        if (!tabla) return;
        doc.autoTable({
            html: tabla,
            startY: y,
            theme: 'grid',
            styles: { fontSize: 8 },
            headStyles: { fillColor: [33, 37, 41] },
            footStyles: { fillColor: [233, 236, 239], textColor: 20, fontStyle: 'bold' },
            margin: { left: margen, right: margen }
        });
        y = doc.lastAutoTable.finalY + 20;
    });

    doc.save(opciones.archivo);
}

// Vista previa de los archivos elegidos en "Enviar mensaje", antes de mandarlos a la API
function vistaPrevia(input, idContenedor) {
    const contenedor = document.getElementById(idContenedor);
    contenedor.innerHTML = '';

    Array.from(input.files).forEach(function (archivo) {
        const tarjeta = document.createElement('div');
        tarjeta.className = 'card mb-2';
        contenedor.appendChild(tarjeta);

        const lector = new FileReader();
        lector.onload = function () {
            let datos = null;
            let tipo = 'JSON invalido';
            try {
                datos = JSON.parse(lector.result);
                const elementos = Array.isArray(datos) ? datos : [datos];
                const tiene = llave => elementos.some(e => e && typeof e === 'object' && llave in e);
                tipo = tiene('NIT') ? 'Actualizacion de RTU'
                     : tiene('REFERENCIA') ? 'Solicitudes de autorizacion'
                     : 'Tipo desconocido';
            } catch (e) { }

            const cuerpo = document.createElement('div');
            cuerpo.className = 'card-body py-2';
            tarjeta.appendChild(cuerpo);

            const titulo = document.createElement('div');
            titulo.innerHTML = '<strong></strong> <span class="badge text-bg-secondary ms-2"></span> <span class="text-muted small ms-2"></span>';
            titulo.querySelector('strong').textContent = archivo.name;
            titulo.querySelector('.badge').textContent = tipo;
            titulo.querySelector('.small').textContent = Array.isArray(datos) ? datos.length + ' elemento(s)' : '';
            cuerpo.appendChild(titulo);

            if (!Array.isArray(datos) || datos.length === 0 || typeof datos[0] !== 'object' || datos[0] === null)
                return;

            // Primeras 3 filas como tabla
            const columnas = Object.keys(datos[0]);
            const tabla = document.createElement('table');
            tabla.className = 'table table-sm table-bordered small mt-2 mb-0';
            const encabezado = tabla.createTHead().insertRow();
            columnas.forEach(c => { encabezado.insertCell().outerHTML = '<th>' + c.replace(/[<>&]/g, '') + '</th>'; });
            const filas = tabla.createTBody();
            datos.slice(0, 3).forEach(function (elemento) {
                const fila = filas.insertRow();
                columnas.forEach(c => { fila.insertCell().textContent = elemento && elemento[c] !== undefined ? elemento[c] : ''; });
            });
            cuerpo.appendChild(tabla);
        };
        lector.readAsText(archivo);
    });
}
