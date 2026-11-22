const CLAVE = 'idioma';
const IDIOMAS = ['es', 'en'];

/**
 * El idioma guardado o, la primera vez, inglés si el navegador está en inglés y español en cualquier otro caso.
 * También lo deja en el atributo lang de la página, para los lectores de pantalla.
 */
export function idiomaInicial() {
  const guardado = leerGuardado();
  const idioma = IDIOMAS.includes(guardado) ? guardado : navigator.language.toLowerCase().startsWith('en') ? 'en' : 'es';
  document.documentElement.lang = idioma;
  return idioma;
}

export function guardar(idioma) {
  try {
    localStorage.setItem(CLAVE, idioma);
  } catch {
    // Sin almacenamiento disponible (por ejemplo, en modo privado), el idioma elegido dura solo esta visita.
  }
}

function leerGuardado() {
  try {
    return localStorage.getItem(CLAVE);
  } catch {
    return null;
  }
}
