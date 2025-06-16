import { fileURLToPath } from "url";
import { dirname, resolve, join } from "path";
import { platform } from "os";

// ES Module equivalent of __dirname
const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

// Determine if we're on Windows or Mac
const isWindows = platform() === "win32";

// Define base directories in a platform-agnostic way
const rootFolder = resolve(__dirname, "..", "..", "..");
const srcFolder = resolve(rootFolder, "src");
const webAppFolder = "Dlw.Kentico.Xperience.WebApp";
const adminFolder = "Dlw.Kentico.Xperience.Admin";
/**
 * Do not change below this comment
 */

const paths = {
  infra: {
    scripts: resolve(rootFolder, "scripts"),
    database: resolve(rootFolder, "database"),
  },
  site: {
    // Use forward slashes for web paths regardless of platform
    styles: "/Styles/",
    scripts: "/Scripts/_shared/",
    fonts: "/Fonts/",
    htmleditor: "/HtmlEditor/",
    projectRoot: rootFolder,    
    infra: {
      scripts: resolve(rootFolder, "scripts"),
      database: resolve(rootFolder, "database"),
    },
  },
  css: {
    // Use join instead of string concatenation
    base: join(rootFolder, "src"),
    sites: [],
  },
  scripts: {
    base: join(rootFolder, "src"),
    sites: [],
  },
  fonts: {
    base: join(rootFolder, "src"),
    sites: [],
  },
  htmleditor: {
    base: join(rootFolder, "src"),
    sites: [],
  },
  html: {
    base: join(rootFolder, "src"),
  },
};

// Helper function to log paths for debugging
function logPaths() {
  console.log(`Running on ${isWindows ? "Windows" : "macOS/Linux"}`);
  console.log("Current directory:", __dirname);
  console.log("Root folder:", rootFolder);
  console.log("Source folder:", srcFolder);
  console.log("WebApp folder:", paths.site.projects.webapp.root);
  console.log("CI Repository:", paths.site.projects.webapp.CIRepository);
  console.log("Scripts folder:", paths.infra.scripts);
}

// Uncomment to debug paths
// logPaths();

export default paths;
