// Sin almacenamiento disponible (por ejemplo, en modo privado), la historia dura solo esta visita.

export function leer(clave) {
  try {
    return localStorage.getItem(clave);
  } catch {
    return null;
  }
}

export function guardar(clave, valor) {
  try {
    localStorage.setItem(clave, valor);
  } catch {
    // Sin espacio o sin permiso no se guarda; la página sigue funcionando.
  }
}

export function borrar(clave) {
  try {
    localStorage.removeItem(clave);
  } catch {
    // Igual que al guardar.
  }
}
