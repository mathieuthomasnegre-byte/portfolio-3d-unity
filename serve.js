// Petit serveur local pour tester l'export WebGL : node serve.js  puis ouvrir http://localhost:8123
// Aucune dépendance : uniquement les modules intégrés de Node.
const http = require("http");
const fs = require("fs");
const path = require("path");

const root = path.join(__dirname, "Build", "WebGL");
const port = Number(process.argv[2]) || Number(process.env.PORT) || 8123;
const types = {
  ".html": "text/html; charset=utf-8",
  ".js": "application/javascript",
  ".wasm": "application/wasm",
  ".data": "application/octet-stream",
  ".json": "application/json",
  ".css": "text/css",
  ".png": "image/png",
  ".svg": "image/svg+xml",
  ".ico": "image/x-icon",
};

http
  .createServer((request, response) => {
    const requested = decodeURIComponent(request.url.split("?")[0]);
    let file = path.normalize(path.join(root, requested));
    if (!file.startsWith(root)) {
      response.writeHead(403).end();
      return;
    }
    // Dossier (l'appli libanais/, par exemple) : on sert son index.html, avec la barre finale pour les chemins relatifs.
    if (fs.existsSync(file) && fs.statSync(file).isDirectory()) {
      if (!requested.endsWith("/")) {
        response.writeHead(301, { Location: requested + "/" }).end();
        return;
      }
      file = path.join(file, "index.html");
    }
    fs.readFile(file, (error, data) => {
      if (error) {
        response.writeHead(404).end("Introuvable");
        return;
      }
      // Les fichiers .unityweb sont compressés en gzip : on le signale au navigateur pour qu'il les
      // décompresse lui-même, et on déduit le type de l'extension d'origine (WebGL.wasm.unityweb → .wasm).
      const gzip = data.length > 1 && data[0] === 0x1f && data[1] === 0x8b;
      const name = path.extname(file) === ".unityweb" ? path.basename(file, ".unityweb") : file;
      const headers = { "Content-Type": types[path.extname(name)] || "application/octet-stream", "Cache-Control": "no-store" };
      if (gzip) headers["Content-Encoding"] = "gzip";
      response.writeHead(200, headers);
      response.end(data);
    });
  })
  .on("error", (error) => {
    console.error(error.code === "EADDRINUSE" ? `Le port ${port} est déjà utilisé : node serve.js <autre port>` : error.message);
    process.exit(1);
  })
  .listen(port, "127.0.0.1", () => console.log(`Portfolio servi sur http://127.0.0.1:${port}`));
