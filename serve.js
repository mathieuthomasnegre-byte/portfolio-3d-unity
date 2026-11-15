// Petit serveur local pour tester l'export WebGL : node serve.js  puis ouvrir http://localhost:8123
// Aucune dépendance : uniquement les modules intégrés de Node.
const http = require("http");
const fs = require("fs");
const path = require("path");

const root = path.join(__dirname, "Build", "WebGL");
const port = Number(process.argv[2]) || 8123;
const types = {
  ".html": "text/html; charset=utf-8",
  ".js": "application/javascript",
  ".wasm": "application/wasm",
  ".data": "application/octet-stream",
  ".json": "application/json",
  ".css": "text/css",
  ".png": "image/png",
  ".ico": "image/x-icon",
};

http
  .createServer((request, response) => {
    const requested = decodeURIComponent(request.url.split("?")[0]);
    const file = path.normalize(path.join(root, requested === "/" ? "index.html" : requested));
    if (!file.startsWith(root)) {
      response.writeHead(403).end();
      return;
    }
    fs.readFile(file, (error, data) => {
      if (error) {
        response.writeHead(404).end("Introuvable");
        return;
      }
      // Les fichiers .unityweb sont compressés en gzip ; le chargeur Unity les décompresse lui-même.
      const type = types[path.extname(file)] || "application/octet-stream";
      response.writeHead(200, { "Content-Type": type, "Cache-Control": "no-store" });
      response.end(data);
    });
  })
  .listen(port, "127.0.0.1", () => console.log(`Portfolio servi sur http://localhost:${port}`));
