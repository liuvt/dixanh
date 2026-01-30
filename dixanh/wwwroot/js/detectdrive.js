// This script detects the user's operating system (macOS or Windows)
// Hiển thị hệ điều hành của người dùng (macOS hoặc Windows)
window.detectOS = () => {
    const isMac = navigator.platform.toLowerCase().includes("mac");
    const className = isMac ? "os-macos" : "os-windows";
    document.documentElement.classList.add(className); // <html>
    console.log(`Detected OS: ${isMac ? "macOS" : "Windows"}`);
};

