// Liberar el enlace en el mismo instante puede cortar la descarga en algunos navegadores.
const ESPERA_ANTES_DE_LIBERAR_MS = 1000;

/**
 * Descarga un texto como archivo. Blazor no puede guardar archivos en el equipo sin pasar por el navegador.
 */
export function descargar(nombre, tipo, contenido) {
  const url = URL.createObjectURL(new Blob([contenido], { type: tipo }));
  const enlace = document.createElement('a');
  enlace.href = url;
  enlace.download = nombre;
  enlace.click();
  setTimeout(() => URL.revokeObjectURL(url), ESPERA_ANTES_DE_LIBERAR_MS);
}
