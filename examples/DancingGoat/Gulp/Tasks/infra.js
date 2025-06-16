//import { exec as _exec } from 'child_process';
import { executeProcess } from '../toolbox.js';
import paths from '../paths.js';

export function updateXperience(callback) {
    executeProcess(`${paths.infra.scripts}\\Gulp-Update-Xperience.ps1 -WorkspaceFolder ${paths.site.projectRoot}`, callback)
}