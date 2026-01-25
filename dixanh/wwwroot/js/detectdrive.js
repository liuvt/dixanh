window.detectOS = () => {
    const isMac = navigator.platform.toLowerCase().includes("mac");
    const className = isMac ? "os-macos" : "os-windows";
    document.documentElement.classList.add(className); // <html>
    console.log(`Detected OS: ${isMac ? "macOS" : "Windows"}`);
};

