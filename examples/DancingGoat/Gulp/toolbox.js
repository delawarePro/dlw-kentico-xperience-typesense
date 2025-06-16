import { spawn } from "child_process";
import { platform } from "os";
import { normalize, sep } from "path";

/**
 * Executes a process and logs output with timestamps in a cross-platform way.
 * @param {string} command - The command to execute.
 * @param {function} callback - Callback function when process exits.
 * @param {Object} [options] - Additional options.
 */
function executeProcess(command, callback, options = {}) {
  const isWindows = platform() === "win32";
  // Normalize path separators based on platform
  command = normalizePathsForPlatform(command);
  // Determine if this is a PowerShell script
  const isPowerShellScript = command.includes(".ps1");
  let cmd, args;
  if (isPowerShellScript) {
    // Use PowerShell for PS1 scripts on both platforms
    if (isWindows) {
      cmd = options.shell || "pwsh.exe";
      args = ["-ExecutionPolicy", "Bypass", "-Command", command];
    } else {
      cmd = "pwsh";
      args = ["-Command", command];
    }
  } else {
    // For non-PS1 commands
    if (isWindows) {
      cmd = options.shell || "pwsh.exe";
      args = ["-ExecutionPolicy", "Bypass", "-Command", command];
    } else {
      cmd = options.shell || "/bin/bash";
      args = ["-c", command];
    }
  }
  consoleWithDate(`🚀 Executing on ${platform()}: ${cmd} ${args.join(" ")}`);
  const spawnOptions = {
    stdio: "pipe",
    shell: options.useShell || false,
  };
  // Add working directory if specified
  if (options.cwd) {
    spawnOptions.cwd = options.cwd;
  }
  // Add environment variables if specified
  if (options.env) {
    spawnOptions.env = { ...process.env, ...options.env };
  }
  const proc = spawn(cmd, args, spawnOptions);
  let stdoutData = "";
  let stderrData = "";
  proc.stdout.on("data", (data) => {
    const output = data.toString().trim();
    stdoutData += output;
    coloredConsole(`[STDOUT] ${output}`, "\x1b[32m"); // Green
  });
  proc.stderr.on("data", (data) => {
    const output = data.toString().trim();
    stderrData += output;
    coloredConsole(`[STDERR] ${output}`, "\x1b[31m"); // Red
  });
  proc.on("error", (err) => {
    coloredConsole(`[ERROR] ${err.message}`, "\x1b[31m");
    callback(1, err, stdoutData, stderrData);
  });
  proc.on("close", (code) => {
    const message = `[EXIT] Process exited with code ${code}`;
    if (code === 0) {
      coloredConsole(message, "\x1b[32m"); // Green if successful
    } else {
      coloredConsole(message, "\x1b[31m"); // Red if failed
    }
    callback(code, null, stdoutData, stderrData);
  });
  return proc; // Return process for potential termination
}

/**
 * Normalizes paths in a command string for the current platform.
 * @param {string} command - The command string containing paths.
 * @returns {string} - The command with normalized paths.
 */
function normalizePathsForPlatform(command) {
  const isWindows = platform() === "win32";
  // Replace Windows-style paths with platform-appropriate paths
  if (!isWindows) {
    // Replace backslashes with forward slashes
    command = command.replace(/\\/g, "/");
    // Replace Windows-style path references
    command = command.replace(/([A-Za-z]):\//g, "/mnt/$1/");
  }
  return command;
}

/**
 * Logs messages with a timestamp.
 * @param {string} message - The message to log.
 */
function consoleWithDate(message) {
  console.log(`[${new Date().toLocaleString()}] ${message}`);
}

/**
 * Logs messages with a specified color.
 * @param {string} message - The message to log.
 * @param {string} color - ANSI color code.
 */
function coloredConsole(message, color) {
  console.log(color + "%s" + "\x1b[0m", message);
}

// Export the function
export { executeProcess };
