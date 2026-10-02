export function downloadBlob(blob: Blob, filename: string): void {
  const url = URL.createObjectURL(blob); // crea una URL temporal, válida solo en esta pestaña
  const link = document.createElement('a');
  link.href = url;
  link.download = filename; // esto es lo que fuerza la descarga en vez de navegar
  link.click();
  URL.revokeObjectURL(url); // liberamos la URL temporal, ya cumplió su función
}
