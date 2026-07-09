import sys
import os
from pathlib import Path
import platform
import warnings
from ctypes import wintypes
import winreg
import ctypes
import customtkinter as ctk
from tkinter import messagebox
import subprocess
import re
import json
import time
import gc
import psutil
import shutil
import threading
from PIL import Image, ImageTk
import cv2
import requests
import glob
import getpass
from packaging.version import Version

##################################################################################
# Created by adasjusk
# GitHub: https://github.com/adasjusk/OrangBooster
# Credits: Vakarux, adasjusk
# License: GPLv3 Allows to edit and redistribute
# Version: 7
# Date: 2025-06-14
# Hide deprecation warnings
warnings.filterwarnings("ignore", category=UserWarning, module="customtkinter")
##################################################################################
# Set up application directories
APP_DIR = Path(os.path.expandvars("%ProgramData%")) / "InterJava-Programs"
TEMP_DIR = Path(os.environ.get("TEMP", os.path.expandvars("%TEMP%")))
STATE_FILE = APP_DIR / "state.json"
BASE_URL = "https://raw.githubusercontent.com/adasjusk/OrangBooster/beta/files/"
REQUIRED_FILES = {
    "video.mp4": BASE_URL + "video.mp4",
    "orange.ico": BASE_URL + "orange.ico",
    "orange.png": BASE_URL + "orange.png",
    "brave.png": BASE_URL + "brave.png",
    "chromium.png": BASE_URL + "chromium.png",
    "arc.png": BASE_URL + "arc.png",
    "orange.json": BASE_URL + "orange.json",
    "state.json": BASE_URL + "state.json"
}
##################################################################################
def verify_required_files():
    missing_files = []
    for filename, url in REQUIRED_FILES.items():
        filepath = APP_DIR / filename
        if not filepath.exists() or os.path.getsize(filepath) == 0:
            try:
                print(f"[*] Downloading {filename}...")
                response = requests.get(url, stream=True)
                response.raise_for_status()
                with open(filepath, 'wb') as f:
                    for chunk in response.iter_content(chunk_size=8192):
                        if chunk:
                            f.write(chunk)
                print(f"[+] Successfully downloaded: {filename}")
            except Exception as e:
                print(f"[!] Failed to download {filename}: {e}")
                missing_files.append(filename)
    return missing_files

def resource_path(relative_path):
    programdata_path = APP_DIR / relative_path
    if not programdata_path.exists():
        if relative_path in REQUIRED_FILES:
            if download_and_save_file(REQUIRED_FILES[relative_path], relative_path):
                print(f"[+] Successfully downloaded required file: {relative_path}")
            else:
                print(f"[!] Failed to download required file: {relative_path}")
    return str(programdata_path)

def get_app_dir():
    """Get the appropriate application directory"""
    programdata = os.getenv('ProgramData')
    if programdata:
        app_dir = Path(programdata) / "InterJava-Programs"
    else:
        localappdata = os.getenv('LOCALAPPDATA')
        if localappdata:
            app_dir = Path(localappdata) / "InterJava-Programs"
        else:
            app_dir = Path.home() / "InterJava-Programs"
    return app_dir


# Set up application directories
APP_DIR = get_app_dir()
STATE_FILE = APP_DIR / "state.json"

def init_program():
    """Initialize program resources and configuration before launching GUI"""
    try:
        # Create program directory if it doesn't exist
        if not APP_DIR.exists():
            print("[*] Creating program directory...")
            os.makedirs(APP_DIR, exist_ok=True)
            subprocess.run(['icacls', str(APP_DIR), '/grant', 'Users:(OI)(CI)F'], check=False)

        # Check and download required files
        missing_files = verify_required_files()
        if missing_files:
            print("[!] Failed to download some required files:")
            for filename in missing_files:
                print(f"    → {filename}")
            return False

        # Initialize default state file if it doesn't exist
        if not STATE_FILE.exists():
            default_state = {
                "tasks": {
                    "NumLock at Startup": True,
                    "Hidden Files": True,
                    "Make BSOD Better": False,
                    "Home and Gallery": True,
                    "Hibernation": True,
                    "Reserved Storage": True,
                    "Edge Browser": True,
                    "Nagle Algorithm For Minecraft": True,
                    "Dark Mode": False,
                    "Ads In Windows": True,
                    "Sticky Keys": True,
                    "Cortana": True,
                    "Bing Search in Start Menu": True,
                    "Copilot AI": True,
                    "Classic Right-Click Menu": False,
                    "BitLocker Encryption": True,
                    "Set taskbar to left": False
                }
              }
            try:
                with open(STATE_FILE, "w", encoding="utf-8") as f:
                    json.dump(default_state, f, indent=2)
                subprocess.run(['icacls', str(STATE_FILE), '/grant', 'Users:(M)'], check=False)
                print(f"[+] Default state file created at: {STATE_FILE}")
            except Exception as e:
                print(f"[!] Failed to write state file: {e}")
                return False

        return True
    except Exception as e:
        print(f"[!] Failed to initialize program: {str(e)}")
        return False


def download_and_save_file(url, filename):
    try:
        response = requests.get(url, stream=True)
        response.raise_for_status()
        filepath = APP_DIR / filename
        with open(filepath, 'wb') as f:
            for chunk in response.iter_content(chunk_size=8192):
                if chunk:
                    f.write(chunk)
        print(f"[+] Successfully downloaded: {filename}")
        return True
    except Exception as e:
        print(f"[!] Failed to download {filename}: {str(e)}")
        return False

def download_required_files():
    try:
        for filename, url in REQUIRED_FILES.items():
            filepath = APP_DIR / filename
            if not filepath.exists() or os.path.getsize(filepath) == 0:
                if not download_and_save_file(url, filename):
                    raise Exception(f"Failed to download {filename}")
        return True
    except Exception as e:
        print(f"[!] Failed to initialize configuration: {str(e)}")
        messagebox.showerror("Error", f"Failed to initialize configuration:\n{str(e)}")
        return False
def check_for_updates():
    # Cmon spin your fucking brain
    print("Executing: Check For Updates")
    REMOTE_URL = "https://raw.githubusercontent.com/adasjusk/OrangBooster/beta/main.py"
    LOCAL_VERSION = "OrangBooster v7.0"
    try:
        response = requests.get(REMOTE_URL, timeout=10)
        response.raise_for_status()
        remote_text = response.text
        remote_version_match = re.search(r'OrangBooster v([\d\.]+)', remote_text)
        if remote_version_match:
            remote_version = remote_version_match.group(1)
        else:
            messagebox.showinfo("Updates", "Could not determine remote version.")
            return
    except Exception as e:
        messagebox.showinfo("Updates", f"Error fetching updates: {e}")
        return
    local_version_match = re.search(r'OrangBooster v([\d\.]+)', LOCAL_VERSION)
    if local_version_match:
        local_version = local_version_match.group(1)
    else:
        messagebox.showinfo("Updates", "Could not determine local version.")
        return
    print(f"Local version: {local_version}")
    print(f"Remote version: {remote_version}")
    if Version(remote_version) > Version(local_version):
        if messagebox.askyesno("Update Available", f"A new version (v{remote_version}) is available. Do you want to update?"):
            dest_path = sys.argv[0]
            try:
                with requests.get(REMOTE_URL, stream=True) as r:
                    r.raise_for_status()
                    with open(dest_path, 'wb') as f:
                        shutil.copyfileobj(r.raw, f)
                messagebox.showinfo("Updates", "Update downloaded. Please restart the application.")
            except Exception as e:
                messagebox.showinfo("Updates", f"Error downloading update: {e}")
        else:
            messagebox.showinfo("Updates", "Update cancelled.")
    else:
        messagebox.showinfo("Updates", "You are running the latest version.")
def is_windows_11():
    try:
        return int(platform.version().split(".")[2]) >= 22000
    except Exception:
        return False
def open_brave_browser():
    # my pet
    url = "https://referrals.brave.com/latest/BraveBrowserSetup-BRV013.exe"
    download_and_install(url, "BraveBrowserSetup-BRV010.exe")
    print("Brave Browser installed successfully.")
def open_ungoogled_chromium():
    # I hate this url every time it changes :(
    url = "https://github.com/ungoogled-software/ungoogled-chromium-windows/releases/download/137.0.7151.119-1.1/ungoogled-chromium_137.0.7151.119-1.1_installer_x64.exe"
    download_and_install(url, "ungoogled-chromium-installer.exe")
    print("Ungoogled Chromium installed successfully.")
def open_arc_browser():
    # disgner browser and nothing to much if it wasn't uwp i love it
    url = "https://releases.arc.net/windows/ArcInstaller.exe"
    download_and_install(url, "ArcInstaller.exe")
    print("Arc Browser installed successfully.")
def download_and_install(url, filename):
    print(f"Downloading {filename}...")
    local_path = os.path.join(os.getenv('TEMP'), filename)
    try:
        response = requests.get(url, stream=True)
        response.raise_for_status()
        with open(local_path, 'wb') as f:
            for chunk in response.iter_content(chunk_size=8192):
                if chunk:
                    f.write(chunk)
        print(f"Installing {filename}...")
        subprocess.run([local_path], check=True)
        try:
            os.remove(local_path)
        except:
            pass
    except Exception as e:
        print(f"Error: {e}")
        if os.path.exists(local_path):
            try:
                os.remove(local_path)
            except:
                pass
BROWSER_FUNCTIONS = {
    "Brave Browser": open_brave_browser,
    "Ungoogled Chromium": open_ungoogled_chromium,
    "Arc Browser": open_arc_browser
}
def optimize_network():
    # Cmon buy adas a new internet not that 0.26 Mbps
    # Stfu vakarux - adas
    print("Executing: Optimize Network")
    commands = [
        'netsh interface teredo set state disabled',
        'reg.exe add "HKLM\\SYSTEM\\CurrentControlSet\\Services\\Tcpip6\\Parameters" /v DisabledComponents /t REG_DWORD /d 255 /f',
        'netsh interface ipv6 set interface * admin=disable',
        'reg add "HKLM\\SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters" /v TcpNoDelay /t REG_DWORD /d 1 /f',
        'reg add "HKLM\\SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters" /v TcpAckFrequency /t REG_DWORD /d 1 /f',
        'reg add "HKLM\\SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters" /v TcpDelAckTicks /t REG_DWORD /d 0 /f',
        'reg add "HKLM\\SYSTEM\\CurrentControlSet\\Services\\LanmanServer\\Parameters" /v IRPStackSize /t REG_DWORD /d 30 /f',
        'ipconfig /flushdns',
        'ipconfig /registerdns',
        'ipconfig /release',
        'ipconfig /renew',
        'netsh winsock reset'
    ]
    for cmd in commands:
        subprocess.run(cmd, shell=True)
def optimize_general_system(self):
    gc.collect()
    process = psutil.Process(os.getpid())
    mem = process.memory_info().rss / (1024 * 1024)
    print(f"Memory after GC: {mem:.2f} MB")
    ps_script = r'''
$RegConnect = [Microsoft.Win32.RegistryKey]::OpenRemoteBaseKey([Microsoft.Win32.RegistryHive]"CurrentUser","$env:COMPUTERNAME")
$RegMouse = $RegConnect.OpenSubKey("Control Panel\Mouse",$true)
$acc_enabled = $RegMouse.GetValue("MouseSpeed")

if ( $acc_enabled -eq 1 ) {
    $RegMouse.SetValue("MouseSpeed","0")
    $RegMouse.SetValue("MouseThreshold1","0")
    $RegMouse.SetValue("MouseThreshold2","0")
    $sys_pvParam = @(0,0,0)
} else {
    $RegMouse.SetValue("MouseSpeed","1")
    $RegMouse.SetValue("MouseThreshold1","6")
    $RegMouse.SetValue("MouseThreshold2","10")
    $sys_pvParam = @(1,6,10)
}
$RegMouse.Close()
$RegConnect.Close()
$code = @'
[DllImport("user32.dll", EntryPoint = "SystemParametersInfo")]
 public static extern bool SystemParametersInfo(uint uiAction, uint uiParam, int[] pvParam, uint fWinIni);
'@
Add-Type $code -name Win32 -NameSpace System
[System.Win32]::SystemParametersInfo(4,0,$sys_pvParam,2)
'''
    subprocess.run(["powershell", "-NoProfile", "-Command", ps_script], shell=True, check=False)
    clean_temp_files()
    print("Optimizing Memory Settings...")
    memory_commands = [
        r'reg add "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management" /v "LargeSystemCache" /t REG_DWORD /d "1" /f',
        r'reg add "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management" /v "DisablePagingExecutive" /t REG_DWORD /d "1" /f',
        r'reg add "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management" /v "SystemCacheDirtyPageThreshold" /t REG_DWORD /d "0" /f'
    ]
    for cmd in memory_commands:
        subprocess.run(cmd, shell=True)
    print("Optimizing Shell Settings...")
    shell_commands = [
        r'reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced" /v "ListviewAlphaSelect" /t REG_DWORD /d "0" /f',
        r'reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced" /v "ListviewShadow" /t REG_DWORD /d "0" /f',
        r'reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced" /v "TaskbarAnimations" /t REG_DWORD /d "0" /f'
    ]
    for cmd in shell_commands:
        subprocess.run(cmd, shell=True)

def clean_temp_files():
    print("Cleaning Temp Files")
    commands = [
        'del /s /f /q C:\\WINDOWS\\Temp\\*.*',
        'del /s /f /q C:\\WINDOWS\\Prefetch\\*.*',
        'del /s /f /q %USERPROFILE%\\AppData\\Local\\Temp\\*.*',
        'cleanmgr.exe /LOWDISK /d C:'
    ]
    for cmd in commands:
        subprocess.run(cmd, shell=True)
def optimize_internet_ping():
    print("Executing: Optimize Internet Ping")
    commands = [
        'netsh interface tcp set global autotuninglevel=disabled',
        'netsh interface tcp set global dca=enabled',
        'netsh interface tcp set global congestionprovider=ctcp',
        'netsh interface tcp set global ecncapability=enabled',
        'netsh interface tcp set global timestamps=enabled'
    ]
    for cmd in commands:
        subprocess.run(cmd, shell=True)
def disable_xbox_stuff():
    try:
        reg_paths = [
            (r"SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled", 0),
            (r"SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR", "GameDVR_Enabled", 0),
            (r"SYSTEM\CurrentControlSet\Services\xbgm", "Start", 4),
            (r"SOFTWARE\Policies\Microsoft\Windows\GameDVR", "AllowGameDVR", 0),
            (r"SOFTWARE\Microsoft\WindowsRuntime\ActivatableClassId\Windows.Gaming.GameBar.PresenceServer.Internal.PresenceWriter", "ActivationType", 0)
        ]
        for path, name, value in reg_paths:
            try:
                key = winreg.CreateKeyEx(winreg.HKEY_LOCAL_MACHINE, path, 0, winreg.KEY_SET_VALUE)
                winreg.SetValueEx(key, name, 0, winreg.REG_DWORD, value)
                winreg.CloseKey(key)
            except WindowsError:
                continue
        xbox_services = [
            "XboxGipSvc",
            "XboxNetApiSvc",
            "XblAuthManager",
            "XblGameSave",
            "XboxGameBarPlugin"
        ]
        for service in xbox_services:
            try:
                subprocess.run(['sc', 'config', service, 'start=', 'disabled'], check=True)
                subprocess.run(['sc', 'stop', service], check=True)
            except subprocess.CalledProcessError:
                continue
        print("[+] Xbox features and services have been disabled")
    except Exception as e:
        print(f"[!] Failed to disable Xbox features: {e}")
        sys.exit(1)
def debloat_system_services():
    try:
        oo_url = "https://code.ravendevteam.org/talon/uninstall_oo.ps1"
        oo_script = APP_DIR / "uninstall_oo.ps1"
        try:
            response = requests.get(oo_url, timeout=10)
            response.raise_for_status()
            with open(oo_script, "wb") as f:
                f.write(response.content)
            print("[+] Downloaded OneDrive/Outlook uninstaller script.")
            subprocess.run([
                "powershell", "-ExecutionPolicy", "Bypass", "-File", str(oo_script)
            ], check=False)
            print("[+] Ran OneDrive/Outlook uninstaller script.")
        except Exception as e:
            print(f"[!] Failed to download or run OneDrive/Outlook uninstaller: {e}")
        services = [
            "AJRouter",                     # AllJoyn Router Service
            "AppVClient",                   # Application Virtualization Client
            "AssignedAccessManagerSvc",     # Assigned Access Manager Service
            "DialogBlockingService",        # Dialog Blocking Service
            "NetTcpPortSharing",            # Net.Tcp Port Sharing Service
            "RemoteAccess",                 # Remote Access Connection Manager
            "RemoteRegistry",               # Remote Registry
            "UevAgentService",              # User Experience Virtualization Service
            "shpamsvc",                     # Microsoft Windows Search Filter Host
            "ssh-agent",                    # OpenSSH Authentication Agent
            "tzautoupdate",                 # Time Zone Auto Update
            "uhssvc",                       # User Health Service
            "DiagTrack",                    # Connected User Experiences and Telemetry
            "dmwappushservice",             # Device Management Wireless Application Protocol
            "RetailDemo",                   # Retail Demo Service
            "lfsvc",                        # Geolocation Service
            "MapsBroker",                   # Downloaded Maps Manager
            "SharedAccess",                 # Internet Connection Sharing
            "WbioSrvc",                     # Windows Biometric Service
            "WMPNetworkSvc",                # Windows Media Player Network Sharing
            "WSearch",                      # Windows Search
            "WMPNetworkSvc",                # Windows Media Player Network Sharing
            "WwanSvc",                      # WWAN AutoConfig
            "Fax",                          # Windows Fax and Scan
            "Spooler",                      # Print Spooler
            "SysMain",                      # Superfetch
            "DiagTrack"                     # Connected User Experiences and Telemetry
        ]
        for service in services:
            try:
                subprocess.run(['sc', 'config', service, 'start=', 'disabled'], check=True)
                subprocess.run(['sc', 'stop', service], check=True)
            except subprocess.CalledProcessError:
                continue
        bloatware_apps = [
            "Microsoft.3DBuilder",

            "Microsoft.Microsoft3DViewer",
            "Microsoft.AppConnector",
            "Microsoft.BingFinance", 
            "Microsoft.BingNews", 
            "Microsoft.BingSports", 
            "Clipchamp.Clipchamp",
            "Microsoft.Todos",
            "MSTeams",
            "MicrosoftCorporationII.QuickAssist",
            "Microsoft.WindowsTerminal",
            "Microsoft.BingTranslator", 
            "Microsoft.BingWeather", 
            "Microsoft.BingFoodAndDrink", 
            "Microsoft.BingHealthAndFitness", 
            "Microsoft.BingTravel", 
            "Microsoft.MinecraftUWP", 
            "Microsoft.GamingServices", 
            "Microsoft.GetHelp", 
            "Microsoft.Getstarted", 
            "Microsoft.Messaging", 
            "Microsoft.Microsoft3DViewer", 
            "Microsoft.MicrosoftSolitaireCollection", 
            "Microsoft.MicrosoftStickyNotes",
            "Microsoft.MixedReality.Portal",
            "Microsoft.NetworkSpeedTest", 
            "Microsoft.News", 
            "Microsoft.Office.Lens", 
            "Microsoft.Office.Sway", 
            "Microsoft.Office.OneNote", 
            "Microsoft.OneConnect", 
            "Microsoft.People", 
            "Microsoft.Print3D", 
            "Microsoft.SkypeApp", 
            "Microsoft.Wallet", 
            "Microsoft.Whiteboard", 
            "Microsoft.WindowsAlarms", 
            "Microsoft.WindowsCamera",
            "microsoft.windowscommunicationsapps", 
            "Microsoft.WindowsFeedbackHub", 
            "Microsoft.WindowsMaps", 
            "Microsoft.WindowsPhone", 
            "Microsoft.WindowsSoundRecorder", 
            "Microsoft.XboxApp", 
            "Microsoft.ConnectivityStore", 
            "Microsoft.CommsPhone", 
            "Microsoft.Xbox.TCUI", 
            "Microsoft.XboxGameOverlay", 
            "Microsoft.XboxGameCallableUI", 
            "Microsoft.XboxSpeechToTextOverlay", 
            "Microsoft.MixedReality.Portal", 
            "Microsoft.XboxIdentityProvider", 
            "Microsoft.ZuneVideo", 
            "Microsoft.YourPhone", 
            "Microsoft.Getstarted", 
            "Microsoft.MicrosoftOfficeHub", 
            "*EclipseManager*", 
            "*ActiproSoftwareLLC*", 
            "Microsoft.MicrosoftEdgeDevToolsClient",
            "Microsoft.Windows.ContentDeliveryManager",
            "Microsoft.Windows.SecureAssessmentBrowser",
            "Microsoft.Windows.NarratorQuickStart",
            "Microsoft.PowerAutomateDesktop",
            "MicrosoftWindows.CrossDevice",
            "Microsoft.Windows.DevHome",
            "Microsoft.BingSearch",
            "Microsoft.ApplicationCompatibilityEnhancements",
            "Microsoft.Edge.GameAssist",
            "Microsoft.ScreenSketch",
            "MicrosoftWindows.Client.WebExperience",
            "MicrosoftWindows.57058570.Speion",
            "MicrosoftWindows.Client.Photon",
            "*AdobeSystemsIncorporated.AdobePhotoshopExpress*", 
            "*Duolingo-LearnLanguagesforFree*", 
            "*PandoraMediaInc*", 
            "*CandyCrush*", 
            "*BubbleWitch3Saga*", 
            "*Wunderlist*", 
            "*Flipboard*", 
            "*Twitter*", 
            "*Facebook*", 
            "*Royal Revolt*", 
            "*Sway*", 
            "*Speed Test*", 
            "*Dolby*", 
            "*Viber*", 
            "*ACGMediaPlayer*", 
            "*Netflix*", 
            "*OneCalendar*", 
            "*LinkedInforWindows*", 
            "*HiddenCityMysteryofShadows*", 
            "*Hulu*", 
            "*HiddenCity*", 
            "*AdobePhotoshopExpress*", 
            "*HotspotShieldFreeVPN*",
            "*Microsoft.Advertising.Xaml*",
            "Microsoft.Windows.Cortana",
            "Microsoft.Windows.ParentalControls",
            "Microsoft.VP9VideoExtensions",
            "Microsoft.XboxGamingOverlay",
            "Microsoft.EdgeDevtoolsPlugin"
        ]
        for app in bloatware_apps:
            try:
                subprocess.run([
                    "powershell",
                    "-Command",
                    f"Get-AppxPackage {app} | Remove-AppxPackage"
                ], check=True)
            except subprocess.CalledProcessError:
                continue
        print("[+] System services have been optimized and bloatware removed")
    except Exception as e:
        print(f"[!] Failed to optimize system services: {e}")
        sys.exit(1)
def optimize_windows_settings(self):
    try:
        performance_settings = [
            (r"Control Panel\Desktop", "MenuShowDelay", "0"),
            (r"Control Panel\Desktop", "WaitToKillAppTimeout", "2000"),
            (r"Control Panel\Desktop", "HungAppTimeout", "1000"),
            (r"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoLowDiskSpaceChecks", 1),
            (r"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "LaunchTo", 1),
            (r"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowTaskViewButton", 0),
            (r"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackDocs", 0),
            (r"Software\Microsoft\Windows\CurrentVersion\Policies\System", "DisableAutomaticRestartSignOn", 1),
            (r"System\CurrentControlSet\Control", "WaitToKillServiceTimeout", "2000")
        ]
        for path, name, value in performance_settings:
            try:
                key = winreg.CreateKeyEx(winreg.HKEY_CURRENT_USER, path, 0, winreg.KEY_SET_VALUE)
                if isinstance(value, str):
                    winreg.SetValueEx(key, name, 0, winreg.REG_SZ, value)
                else:
                    winreg.SetValueEx(key, name, 0, winreg.REG_DWORD, value)
                winreg.CloseKey(key)
            except WindowsError:
                continue
        features_to_disable = [
            "WindowsMediaPlayer",
            "Printing-PrintToPDFServices-Features",
            "Printing-XPSServices-Features",
            "WorkFolders-Client"
        ]
        for feature in features_to_disable:
            try:
                subprocess.run([
                    "dism",
                    "/online",
                    "/disable-feature",
                    f"/featurename:{feature}",
                    "/norestart"
                ], check=True)
            except subprocess.CalledProcessError:
                continue
        print("[+] Windows settings have been optimized")
    except Exception as e:
        print(f"[!] Failed to optimize Windows settings: {e}")
        self.run_operation("Disabling Background Apps", [
            r'reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications" /v "GlobalUserDisabled" /t REG_DWORD /d "1" /f',
            r'reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Search" /v "BackgroundAppGlobalToggle" /t REG_DWORD /d "0" /f'
        ])
def make_everything():
    print("Executing: Make Everything In list")
    try:
        optimize_network()
        optimize_general_system()
        optimize_internet_ping()
        disable_xbox_stuff()
        debloat_system_services()
        optimize_windows_settings()
        
        print("[+] All optimizations have been completed successfully")
    except Exception as e:
        print(f"[!] Error during optimization process: {e}")
        sys.exit(1)
def enable_copilot_ai():
    run_powershell('winget install --id Microsoft.Copilot --silent')
    subprocess.run([
        "reg", "add",
        r"HKCU\Software\Microsoft\Windows\Shell\Copilot\BingChat",
        "/v", "IsUserEligible",
        "/t", "REG_DWORD",
        "/d", "1",
        "/f"
    ], shell=True)
    subprocess.run([
        "reg", "add",
        r"HKCU\Software\Microsoft\Windows\Shell\Copilot",
        "/v", "IsCopilotAvailable",
        "/t", "REG_DWORD",
        "/d", "1",
        "/f"
    ], shell=True)
    subprocess.run([
        "reg", "add",
        r"HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced",
        "/v", "ShowCopilotButton",
        "/t", "REG_DWORD",
        "/d", "1",
        "/f"
    ], shell=True)
    subprocess.run([
        "reg", "add",
        r"HKEY_CURRENT_USER\Software\Policies\Microsoft\Windows\WindowsCopilot",
        "/v", "TurnOffWindowsCopilot",
        "/t", "REG_DWORD",
        "/d", "0",
        "/f"
    ], shell=True)
    subprocess.run(
        ["dism", "/online", "/Enable-Feature", "/FeatureName:Recall", "/All"],
        shell=True
    )
def disable_copilot_ai():
    run_powershell(
        'Get-AppxPackage -AllUsers | Where-Object {$_.Name -Like "*Microsoft.Copilot*"} '
        '| Remove-AppxPackage -AllUsers -ErrorAction Continue'
    )
    run_powershell('winget uninstall --id Microsoft.Copilot_8wekyb3d8bbwe --silent')
    run_powershell(
        'Get-AppxPackage -AllUsers | Where-Object {$_.Name -Like "*MicrosoftWindows.Client.CoreAI*"} | Remove-AppxPackage -AllUsers -ErrorAction Continue'
    )
    subprocess.run([
        "reg", "add",
        r"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsAI",
        "/v", "DisableAllDataAnalysis",
        "/t", "REG_DWORD",
        "/d", "0",
        "/f"
    ], shell=True)
    subprocess.run([
        "reg", "add",
        r"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsAI",
        "/v", "AllowRecallEnablement",
        "/t", "REG_DWORD",
        "/d", "0",
        "/f"
    ], shell=True)
    # Paint: DisableImageCreator
    subprocess.run([
        "reg", "add",
        r"HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\Paint",
        "/v", "DisableImageCreator",
        "/t", "REG_DWORD",
        "/d", "1",
        "/f"
    ], shell=True)
    # Copilot registry disables
    subprocess.run([
        "reg", "add",
        r"HKCU\\Software\\Microsoft\\Windows\\Shell\\Copilot\\BingChat",
        "/v", "IsUserEligible",
        "/t", "REG_DWORD",
        "/d", "0",
        "/f"
    ], shell=True)
    subprocess.run([
        "reg", "add",
        r"HKCU\\Software\\Microsoft\\Windows\\Shell\\Copilot",
        "/v", "IsCopilotAvailable",
        "/t", "REG_DWORD",
        "/d", "0",
        "/f"
    ], shell=True)
    subprocess.run([
        "reg", "add",
        r"HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced",
        "/v", "ShowCopilotButton",
        "/t", "REG_DWORD",
        "/d", "0",
        "/f"
    ], shell=True)
    subprocess.run([
        "reg", "add",
        r"HKEY_CURRENT_USER\\Software\\Policies\\Microsoft\\Windows\\WindowsCopilot",
        "/v", "TurnOffWindowsCopilot",
        "/t", "REG_DWORD",
        "/d", "1",
        "/f"
    ], shell=True)
    subprocess.run(
        ["dism", "/online", "/Disable-Feature", "/FeatureName:Recall"],
        shell=True
    )
def enable_cortana():
    print("[*] Reinstalling Cortana from Microsoft Store...")
    try:
        subprocess.run([
            "powershell",
            "-Command",
            "Get-AppxPackage -allusers Microsoft.549981C3F5F10 -ErrorAction SilentlyContinue || "
            "Invoke-WebRequest https://aka.ms/getcortana -OutFile $env:TEMP\\Cortana.msixbundle; "
            "Add-AppxPackage -Path $env:TEMP\\Cortana.msixbundle"
        ], check=True)
        print("[+] Cortana installation attempted.")
    except subprocess.CalledProcessError as e:
        print(f"[!] Failed to reinstall Cortana: {e}")
    print("[*] Enabling Cortana via Registry...")
    try:
        reg_path = r"SOFTWARE\Policies\Microsoft\Windows\Windows Search"
        key = winreg.CreateKeyEx(winreg.HKEY_LOCAL_MACHINE, reg_path, 0, winreg.KEY_SET_VALUE)
        winreg.SetValueEx(key, "AllowCortana", 0, winreg.REG_DWORD, 1)
        winreg.CloseKey(key)
        print("[+] Cortana enabled in registry.")
    except PermissionError:
        print("[!] Permission denied. Run as Administrator.")
        sys.exit(1)
    except Exception as e:
        print(f"[!] Error: {e}")
def disable_cortana():
    print("[*] Disabling Cortana via Registry...")
    try:
        reg_path = r"SOFTWARE\Policies\Microsoft\Windows\Windows Search"
        try:
            key = winreg.CreateKeyEx(winreg.HKEY_LOCAL_MACHINE, reg_path, 0, winreg.KEY_SET_VALUE)
        except PermissionError:
            print("[!] Permission denied. Try running this script as Administrator.")
            sys.exit(1)
        winreg.SetValueEx(key, "AllowCortana", 0, winreg.REG_DWORD, 0)
        winreg.CloseKey(key)
        print("[+] Cortana disabled in registry.")
    except Exception as e:
        print(f"[!] Failed to disable Cortana in registry: {e}")

    print("[*] Attempting to uninstall Cortana (UWP)...")
    try:
        subprocess.run([
            "powershell",
            "-Command",
            "Get-AppxPackage -allusers Microsoft.549981C3F5F10 | Remove-AppxPackage"
        ], check=True)
        print("[+] Cortana uninstalled successfully.")
    except subprocess.CalledProcessError as e:
        print(f"[!] Failed to uninstall Cortana: {e}")

def enable_nagle_algorithm():
    try:
        reg_paths = [
            r"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces",
            r"SOFTWARE\Microsoft\MSMQ\Parameters"
        ]
        
        for base_path in reg_paths:
            try:
                key = winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE, base_path, 0, winreg.KEY_READ)
                i = 0
                while True:
                    try:
                        subkey_name = winreg.EnumKey(key, i)
                        subkey_path = f"{base_path}\\{subkey_name}"
                        subkey = winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE, subkey_path, 0, winreg.KEY_SET_VALUE)
                        try:
                            winreg.DeleteValue(subkey, "TCPNoDelay")
                            winreg.DeleteValue(subkey, "TcpAckFrequency")
                        except WindowsError:
                            pass
                            
                        winreg.CloseKey(subkey)
                        i += 1
                    except WindowsError:
                        break
                winreg.CloseKey(key)
            except WindowsError:
                continue
                
        print("[+] Nagle's Algorithm has been enabled")
    except Exception as e:
        print(f"[!] Failed to enable Nagle's Algorithm: {e}")
        sys.exit(1)
def disable_nagle_algorithm():
    try:
        reg_paths = [
            r"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces",
            r"SOFTWARE\Microsoft\MSMQ\Parameters"
        ]
        for base_path in reg_paths:
            try:
                key = winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE, base_path, 0, winreg.KEY_READ)
                i = 0
                while True:
                    try:
                        subkey_name = winreg.EnumKey(key, i)
                        subkey_path = f"{base_path}\\{subkey_name}"
                        subkey = winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE, subkey_path, 0, winreg.KEY_SET_VALUE)
                        winreg.SetValueEx(subkey, "TCPNoDelay", 0, winreg.REG_DWORD, 1)
                        winreg.SetValueEx(subkey, "TcpAckFrequency", 0, winreg.REG_DWORD, 1)
                        winreg.CloseKey(subkey)
                        i += 1
                    except WindowsError:
                        break
                winreg.CloseKey(key)
            except WindowsError:
                continue
        print("[+] Nagle's Algorithm has been disabled for better gaming performance")
    except Exception as e:
        print(f"[!] Failed to disable Nagle's Algorithm: {e}")
        sys.exit(1)
def enable_windows_dark_mode():
    try:
        reg_paths = [
            (r"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", 0),
            (r"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", 0),
            (r"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize", "EnableTransparency", 1)
        ]
        for path, name, value in reg_paths:
            key = winreg.CreateKeyEx(winreg.HKEY_CURRENT_USER, path, 0, winreg.KEY_SET_VALUE)
            winreg.SetValueEx(key, name, 0, winreg.REG_DWORD, value)
            winreg.CloseKey(key)
        HWND_BROADCAST = 0xFFFF
        WM_SETTINGCHANGE = 0x001A
        SMTO_ABORTIFHUNG = 0x0002
        ctypes.windll.user32.SendMessageTimeoutW(HWND_BROADCAST, WM_SETTINGCHANGE, 0, 
            'ImmersiveColorSet', SMTO_ABORTIFHUNG, 1000, None)
        print("[+] Dark mode has been enabled")
    except Exception as e:
        print(f"[!] Failed to enable dark mode: {e}")
        sys.exit(1)
def disable_windows_dark_mode():
    try:
        reg_paths = [
            (r"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", 1),
            (r"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", 1),
            (r"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize", "EnableTransparency", 1)
        ]
        for path, name, value in reg_paths:
            key = winreg.CreateKeyEx(winreg.HKEY_CURRENT_USER, path, 0, winreg.KEY_SET_VALUE)
            winreg.SetValueEx(key, name, 0, winreg.REG_DWORD, value)
            winreg.CloseKey(key)
        HWND_BROADCAST = 0xFFFF
        WM_SETTINGCHANGE = 0x001A
        SMTO_ABORTIFHUNG = 0x0002
        ctypes.windll.user32.SendMessageTimeoutW(HWND_BROADCAST, WM_SETTINGCHANGE, 0, 
            'ImmersiveColorSet', SMTO_ABORTIFHUNG, 1000, None)
        print("[+] Light mode has been enabled")
    except Exception as e:
        print(f"[!] Failed to enable light mode: {e}")
        sys.exit(1)
def enable_bing_search():
    for path, name, val in [
        (r"HKCU\Software\Microsoft\Windows\CurrentVersion\Search", "BingSearchEnabled", "1"),
        (r"HKCU\Software\Microsoft\Windows\CurrentVersion\Search", "CortanaConsent", "1"),
        (r"HKCU\Software\Policies\Microsoft\Windows\Explorer", "DisableSearchBoxSuggestions", "0"),
    ]:
        subprocess.run(
            ["reg", "add", path, "/v", name, "/t", "REG_DWORD", "/d", val, "/f"],
            shell=True, check=True
        )
    subprocess.run(
        ["powershell", "-NoProfile", "-Command",
        "Get-AppxPackage -AllUsers *Bing* | Foreach {Add-AppxPackage -DisableDevelopmentMode "
        "-Register \"$($_.InstallLocation)\\AppxManifest.xml\"}"],
        shell=True, check=False
    )
def disable_bing_search():
    for path, name, val in [
        (r"HKCU\Software\Microsoft\Windows\CurrentVersion\Search", "BingSearchEnabled", "0"),
        (r"HKCU\Software\Microsoft\Windows\CurrentVersion\Search", "CortanaConsent", "0"),
        (r"HKCU\Software\Policies\Microsoft\Windows\Explorer", "DisableSearchBoxSuggestions", "1"),
    ]:
        subprocess.run(
            ["reg", "add", path, "/v", name, "/t", "REG_DWORD", "/d", val, "/f"],
            shell=True, check=True
        )
    subprocess.run(
        ["powershell", "-NoProfile", "-Command",
        "Get-AppxPackage -AllUsers *Bing* | Remove-AppxPackage -AllUsers -ErrorAction SilentlyContinue"],
        shell=True, check=False
    )
    subprocess.run(["taskill", "SearchUI"], shell=True, check=False)
def enable_classic_menu():
    # Enable classic context menu by creating the required registry key and value
    subprocess.run([
        "reg", "add", r"HKCU\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}", "/f"
    ], shell=True, check=False)
    subprocess.run([
        "reg", "add", r"HKCU\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32", "/f", "/ve"
    ], shell=True, check=False)
    print("[+] Classic context menu has been enabled (Windows 11 style disabled)")

def disable_classic_menu():
    # Restore modern context menu by deleting the registry key
    subprocess.run([
        "reg", "delete", r"HKCU\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}", "/f"
    ], shell=True, check=False)
    print("[+] Modern context menu has been restored (classic menu disabled)")
def enable_ads():
    try:
        reg_paths = [
            (r"SOFTWARE\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", 1),
            (r"SOFTWARE\Microsoft\Windows\CurrentVersion\Privacy", "TailoredExperiencesWithDiagnosticDataEnabled", 1),
            (r"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SoftLandingEnabled", 1),
            (r"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338388Enabled", 1),
            (r"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338389Enabled", 1),
            (r"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SystemPaneSuggestionsEnabled", 1)
        ]
        for path, name, value in reg_paths:
            key = winreg.CreateKeyEx(winreg.HKEY_LOCAL_MACHINE, path, 0, winreg.KEY_SET_VALUE)
            winreg.SetValueEx(key, name, 0, winreg.REG_DWORD, value)
            winreg.CloseKey(key)
        print("[+] Windows advertising features have been enabled")
    except Exception as e:
        print(f"[!] Failed to enable advertising features: {e}")
        sys.exit(1)
def disable_ads():
    try:
        reg_paths = [
            (r"SOFTWARE\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", 0),
            (r"SOFTWARE\Microsoft\Windows\CurrentVersion\Privacy", "TailoredExperiencesWithDiagnosticDataEnabled", 0),
            (r"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SoftLandingEnabled", 0),
            (r"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338388Enabled", 0),
            (r"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338389Enabled", 0),
            (r"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SystemPaneSuggestionsEnabled", 0)
        ]
        for path, name, value in reg_paths:
            try:
                key = winreg.CreateKeyEx(winreg.HKEY_LOCAL_MACHINE, path, 0, winreg.KEY_SET_VALUE)
                if isinstance(value, str):
                    winreg.SetValueEx(key, name, 0, winreg.REG_SZ, value)
                else:
                    winreg.SetValueEx(key, name, 0, winreg.REG_DWORD, value)
                winreg.CloseKey(key)
            except WindowsError as e:
                print(f"Failed to set registry value {name} in {path}: {e}")
                continue
        print("[+] Windows advertising features have been disabled")
    except Exception as e:
        print(f"[!] Failed to disable advertising features: {e}")
        sys.exit(1)
def enable_bitlocker():
    try:
        subprocess.run(['sc', 'config', 'BDESVC', 'start=', 'auto'], check=True)
        subprocess.run(['net', 'start', 'BDESVC'], check=True)
        print("[+] BitLocker service has been enabled")
        print("[*] Note: You'll need to configure BitLocker through Windows Settings")
    except subprocess.CalledProcessError as e:
        print(f"[!] Failed to enable BitLocker service: {e}")
        sys.exit(1)
def disable_bitlocker():
    try:
        manage_bde = subprocess.run(['manage-bde', '-status'], capture_output=True, text=True)
        if "Protection On" in manage_bde.stdout:
            drives = re.findall(r"([A-Z]:)", manage_bde.stdout)
            for drive in drives:
                if "Protection On" in manage_bde.stdout:
                    print(f"[*] Decrypting {drive}...")
                    subprocess.run(['manage-bde', '-off', drive], check=True)
        subprocess.run(['sc', 'config', 'BDESVC', 'start=', 'disabled'], check=True)
        subprocess.run(['net', 'stop', 'BDESVC'], check=True)
        print("[+] BitLocker has been disabled")
    except subprocess.CalledProcessError as e:
        print(f"[!] Failed to disable BitLocker: {e}")
        sys.exit(1)
def enable_taskbar_left():
    try:
        reg_path = r"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced"
        key = winreg.OpenKey(winreg.HKEY_CURRENT_USER, reg_path, 0, winreg.KEY_SET_VALUE)
        winreg.SetValueEx(key, "TaskbarAl", 0, winreg.REG_DWORD, 0)
        winreg.CloseKey(key)
        print("[+] Registry updated: Taskbar icons will align to the left.")
    except Exception as e:
        print(f"[!] Failed writing registry: {e}")
        sys.exit(1)
    try:
        subprocess.run(["taskkill", "/f", "/im", "explorer.exe"], check=True)
        subprocess.Popen(["explorer.exe"])
        print("[+] Explorer restarted. Taskbar alignment updated.")
    except subprocess.CalledProcessError as e:
        print(f"[!] Could not restart Explorer: {e}")
    except Exception as e:
        print(f"[!] Unexpected error while restarting Explorer: {e}")
def disable_taskbar_left():
    try:
        reg_path = r"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced"
        key = winreg.OpenKey(winreg.HKEY_CURRENT_USER, reg_path, 0, winreg.KEY_SET_VALUE)
        winreg.SetValueEx(key, "TaskbarAl", 0, winreg.REG_DWORD, 1)
        winreg.CloseKey(key)
        print("[+] Registry updated: Taskbar icons will be center-aligned.")
    except Exception as e:
        print(f"[!] Registry write failed: {e}")
        sys.exit(1)
    try:
        subprocess.run(["taskkill", "/f", "/im", "explorer.exe"], check=True)
        subprocess.Popen(["explorer.exe"])
        print("[+] Explorer restarted. Taskbar alignment updated.")
    except subprocess.CalledProcessError as e:
        print(f"[!] Failed to restart Explorer: {e}")
def enable_sticky_keys():
    try:
        reg_path = r"Control Panel\Accessibility\StickyKeys"
        key = winreg.CreateKeyEx(winreg.HKEY_CURRENT_USER, reg_path, 0, winreg.KEY_SET_VALUE)
        winreg.SetValueEx(key, "Flags", 0, winreg.REG_DWORD, 510)
        winreg.CloseKey(key)
        reg_path = r"Control Panel\Accessibility\Keyboard Response"
        key = winreg.CreateKeyEx(winreg.HKEY_CURRENT_USER, reg_path, 0, winreg.KEY_SET_VALUE)
        winreg.SetValueEx(key, "Flags", 0, winreg.REG_DWORD, 126)
        winreg.CloseKey(key)
        reg_path = r"Control Panel\Accessibility\ToggleKeys"
        key = winreg.CreateKeyEx(winreg.HKEY_CURRENT_USER, reg_path, 0, winreg.KEY_SET_VALUE)
        winreg.SetValueEx(key, "Flags", 0, winreg.REG_DWORD, 62)
        winreg.CloseKey(key)
        print("[+] Sticky Keys features have been enabled")
    except Exception as e:
        print(f"[!] Failed to enable accessibility features: {e}")
        sys.exit(1)
def disable_sticky_keys():
    try:
        import winreg
        reg_settings = [
            (r"Control Panel\Accessibility\StickyKeys", {
                "Flags": 506,
                "HotKeyActive": 0,
                "HotKeySound": 0,
                "ConfirmOnHotKey": 0
            }),
            (r"Control Panel\Accessibility\Keyboard Response", {
                "Flags": 122,
                "HotKeyActive": 0,
                "HotKeySound": 0,
                "ConfirmOnHotKey": 0
            }),
            (r"Control Panel\Accessibility\ToggleKeys", {
                "Flags": 58,
                "HotKeyActive": 0,
                "HotKeySound": 0,
                "ConfirmOnHotKey": 0
            }),
            (r"Control Panel\Accessibility\FilterKeys", {
                "Flags": 34,
                "HotKeyActive": 0,
                "HotKeySound": 0,
                "ConfirmOnHotKey": 0
            })
        ]
        for reg_path, values in reg_settings:
            key = winreg.CreateKeyEx(winreg.HKEY_CURRENT_USER, reg_path, 0, winreg.KEY_SET_VALUE)
            for name, val in values.items():
                winreg.SetValueEx(key, name, 0, winreg.REG_DWORD, val)
            winreg.CloseKey(key)
        print("[+] Sticky Keys and related accessibility features have been disabled")
    except Exception as e:
        print(f"[!] Failed to disable accessibility features: {e}")
        sys.exit(1)

def run_powershell(cmd: str):
    subprocess.run(["powershell", "-NoProfile", "-Command", cmd], shell=True, check=False)
def enable_numlock():
    subprocess.run(["reg", "add", r"HKEY_USERS\.DEFAULT\Control Panel\Keyboard", "/v", "InitialKeyboardIndicators", "/t", "REG_SZ", "/d", "2", "/f"], shell=True)
def disable_numlock():
    subprocess.run(["reg", "add", r"HKEY_USERS\.DEFAULT\Control Panel\Keyboard", "/v", "InitialKeyboardIndicators", "/t", "REG_SZ", "/d", "0", "/f"], shell=True)
def show_hidden_files():
    subprocess.run(["reg", "add", r"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "/v", "Hidden", "/t", "REG_DWORD", "/d", "1", "/f"], shell=True)
    subprocess.run(["reg", "add", r"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "/v", "ShowSuperHidden", "/t", "REG_DWORD", "/d", "1", "/f"], shell=True)
def hide_hidden_files():
    subprocess.run(["reg", "add", r"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "/v", "Hidden", "/t", "REG_DWORD", "/d", "0", "/f"], shell=True)
    subprocess.run(["reg", "add", r"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "/v", "ShowSuperHidden", "/t", "REG_DWORD", "/d", "0", "/f"], shell=True)
def enable_bsod_parameters():
    subprocess.run(["reg", "add", r"HKLM\SYSTEM\CurrentControlSet\Control\CrashControl", "/v", "DisplayParameters", "/t", "REG_DWORD", "/d", "4", "/f"], shell=True)
    subprocess.run(["reg", "add", r"HKLM\SYSTEM\CurrentControlSet\Control\CrashControl", "/v", "DisablePromo", "/t", "REG_DWORD", "/d", "1", "/f"], shell=True)
def disable_bsod_parameters():
    subprocess.run(["reg", "delete", r"HKLM\SYSTEM\CurrentControlSet\Control\CrashControl", '/f'], shell=True)
    subprocess.run(["reg", "delete", r"HKLM\SYSTEM\CurrentControlSet\Control\CrashControl", '/f'], shell=True)
def remove_home_gallery():
    try:
        # Set HubMode to 1 to hide Home in Explorer (Windows 11)
        subprocess.run([
            'reg', 'add', r'HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer',
            '/v', 'HubMode', '/t', 'REG_DWORD', '/d', '1', '/f'
        ], shell=True)
        # Remove Home and Gallery NameSpace keys
        subprocess.run(['reg', 'delete', r'HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\{e88865ea-0e1c-4e20-9aa6-edcd0212c87c}', '/f'], shell=True)
        subprocess.run(['reg', 'delete', r'HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\{f874310e-b6b7-47dc-bc84-b9e6b38f5903}', '/f'], shell=True)
        # Optionally, set LaunchTo to 1 (This PC)
        subprocess.run(['reg', 'add', r'HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced', '/v', 'LaunchTo', '/t', 'REG_DWORD', '/d', '1', '/f'], shell=True)
        # Restart Explorer
        try:
            subprocess.run(["taskkill", "/f", "/im", "explorer.exe"], check=True)
            subprocess.Popen(["explorer.exe"])
            print("[+] Explorer restarted.")
        except Exception as e:
            print(f"[!] Failed to restart Explorer: {e}")
    except Exception as e:
        print(f"[!] Failed to remove Home and Gallery: {e}")
def restore_home_gallery():
    subprocess.run(['reg', 'add', r'HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\{e88865ea-0e1c-4e20-9aa6-edcd0212c87c}', '/f'], shell=True)
    subprocess.run(['reg', 'add', r'HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\{f874310e-b6b7-47dc-bc84-b9e6b38f5903}', '/f'], shell=True)
    subprocess.run(['reg', 'add', r'HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced', '/f', '/v', 'LaunchTo', '/t', 'REG_DWORD', '/d', '0'], shell=True)
    try:
        subprocess.run(["taskkill", "/f", "/im", "explorer.exe"], check=True)
        subprocess.Popen(["explorer.exe"])
        print("[+] Explorer restarted.")
    except Exception as e:
        print(f"[!] Failed to restart Explorer: {e}")
def disable_hibernation():
    subprocess.run(["powercfg", "-h", "off"], shell=True)
def enable_hibernation():
    subprocess.run(["powercfg", "-h", "on"], shell=True)
def disable_reserved_storage():
    subprocess.run(['powershell', '-Command', 'Set-WindowsReservedStorageState -State Disabled'], shell=True)
def enable_reserved_storage():
    subprocess.run(['powershell', '-Command', 'Set-WindowsReservedStorageState -State Enabled'], shell=True)
def _download_and_verify(script_name):
    """
    Download the specified script to the APP_DIR and return its path.
    For 'edge_vanisher', downloads from a known URL.
    """
    script_urls = {
        "edge_vanisher": "https://raw.githubusercontent.com/adasjusk/OrangBooster/beta/files/edge_vanisher.ps1"
    }
    if script_name not in script_urls:
        return None
    url = script_urls[script_name]
    script_path = APP_DIR / f"{script_name}.ps1"
    try:
        response = requests.get(url, timeout=10)
        response.raise_for_status()
        with open(script_path, "wb") as f:
            f.write(response.content)
        return str(script_path)
    except Exception as e:
        print(f"[!] Failed to download {script_name}: {e}")
        return None

def debloat_edge():
    # I hate microsoft so remove your shitty browser
    try:
        script = _download_and_verify("edge_vanisher")
        if not script:
            raise Exception("Failed to download Edge Vanisher script")
        # Run the PowerShell script to debloat Edge
        cmd = f"Set-ExecutionPolicy Bypass -Scope Process -Force; & '{script}'; exit"
        proc = subprocess.run(
            ["powershell", "-Command", cmd],
            capture_output=True, text=True
        )
        if proc.returncode != 0:
            print(f"[!] Edge Vanisher script error: {proc.stderr or proc.stdout}")
        # Always apply registry modifications
        registry_modifications = [
            (winreg.HKEY_LOCAL_MACHINE,
             r"SOFTWARE\Policies\Microsoft\Edge",
             "HideFirstRunExperience", winreg.REG_DWORD, 1),
            (winreg.HKEY_LOCAL_MACHINE,
             r"SOFTWARE\Policies\Microsoft\Edge",
             "PreventFirstRunPage", winreg.REG_DWORD, 1),
            (winreg.HKEY_LOCAL_MACHINE,
             r"SOFTWARE\Policies\Microsoft\Edge",
             "AutoImportAtFirstRun", winreg.REG_DWORD, 0),
            (winreg.HKEY_LOCAL_MACHINE,
             r"SOFTWARE\Policies\Microsoft\EdgeUpdate",
             "UpdateDefault", winreg.REG_DWORD, 0),
        ]
        for root, path, name, val_type, val in registry_modifications:
            try:
                with winreg.CreateKeyEx(root, path, 0, winreg.KEY_SET_VALUE) as key:
                    winreg.SetValueEx(key, name, 0, val_type, val)
            except Exception as e:
                print(f"[!] Registry set failed for {name} in {path}: {e}")
        # Block Edge URLs in hosts file if not already present
        edge_urls = [
            "c2rsetup.officeapps.live.com",
            "edgesetup.microsoft.com",
            "edge.microsoft.com",
            "msedge.sf.dl.delivery.mp.microsoft.com"
        ]
        hosts_path = r"C:\Windows\System32\drivers\etc\hosts"
        try:
            with open(hosts_path, 'r', encoding='utf-8', errors='ignore') as hosts_file:
                hosts_content = hosts_file.read()
        except Exception:
            hosts_content = ""
        try:
            with open(hosts_path, 'a', encoding='utf-8', errors='ignore') as hosts_file:
                for url in edge_urls:
                    if url not in hosts_content:
                        hosts_file.write(f"\n127.0.0.1 {url}")
        except Exception as e:
            print(f"[!] Could not write to hosts file: {e}")
        print("[+] Microsoft Edge has been debloated.")
    except Exception as e:
        print(f"[!] Failed to debloat Edge: {e}")
        script = _download_and_verify("edge_vanisher")
        if not script:
            raise Exception("Failed to download Edge Vanisher script")
        cmd = f"Set-ExecutionPolicy Bypass -Scope Process -Force; & '{script}'; exit"
        proc = subprocess.run(
            ["powershell", "-Command", cmd],
            capture_output=True, text=True
        )
        if proc.returncode != 0:
            raise Exception(f"Edge Vanisher failed: {proc.stderr or proc.stdout}")
        registry_modifications = [
            (winreg.HKEY_LOCAL_MACHINE,
             r"SOFTWARE\Policies\Microsoft\Edge",
             "HideFirstRunExperience", winreg.REG_DWORD, 1),
            (winreg.HKEY_LOCAL_MACHINE,
             r"SOFTWARE\Policies\Microsoft\Edge",
             "PreventFirstRunPage", winreg.REG_DWORD, 1),
            (winreg.HKEY_LOCAL_MACHINE,
             r"SOFTWARE\Policies\Microsoft\Edge",
             "AutoImportAtFirstRun", winreg.REG_DWORD, 0),
            (winreg.HKEY_LOCAL_MACHINE,
             r"SOFTWARE\Policies\Microsoft\EdgeUpdate",
             "UpdateDefault", winreg.REG_DWORD, 0),
        ]
        for root, path, name, val_type, val in registry_modifications:
            try:
                with winreg.CreateKeyEx(root, path, 0, winreg.KEY_SET_VALUE) as key:
                    winreg.SetValueEx(key, name, 0, val_type, val)
            except Exception as e:
                pass
        edge_urls = [
            "c2rsetup.officeapps.live.com",
            "edgesetup.microsoft.com",
            "edge.microsoft.com",
            "msedge.sf.dl.delivery.mp.microsoft.com"
        ]
        hosts_path = r"C:\Windows\System32\drivers\etc\hosts"
        with open(hosts_path, 'a') as hosts_file:
            for url in edge_urls:
                hosts_file.write(f"\n127.0.0.1 {url}")

def restore_edge():
    try:
        # Attempt to reinstall Microsoft Edge using PowerShell
        subprocess.run([
            "powershell",
            "-Command",
            "Get-AppxPackage -allusers Microsoft.MicrosoftEdge | Foreach {Add-AppxPackage -DisableDevelopmentMode -Register \"$($_.InstallLocation)\\AppXManifest.xml\"}"
        ], check=True)
        print("[+] Microsoft Edge has been restored.")
    except Exception as e:
        print(f"[!] Failed to restore Microsoft Edge: {e}")

def clear_browser_cache_and_history():
    user = getpass.getuser()
    browser_paths = {
        'Brave': [
            Path(os.environ['LOCALAPPDATA']) / 'BraveSoftware' / 'Brave-Browser' / 'User Data' / 'Default',
        ],
        'LibreWolf': [
            Path(os.environ['APPDATA']) / 'librewolf' / 'Profiles',
        ],
        'Firefox': [
            Path(os.environ['APPDATA']) / 'Mozilla' / 'Firefox' / 'Profiles',
        ],
    }
    brave_targets = ['Cache', 'Code Cache', 'Service Worker', 'History', 'History Provider Cache', 'Visited Links']
    ff_cache_patterns = ['cache2', 'startupCache']
    ff_history_files = ['places.sqlite', 'places.sqlite-wal', 'places.sqlite-shm']
    removed = []
    for browser, paths in browser_paths.items():
        for base in paths:
            if not base.exists():
                continue
            if browser == 'Brave':
                for target in brave_targets:
                    target_path = base / target
                    if target_path.exists():
                        try:
                            if target_path.is_dir():
                                shutil.rmtree(target_path, ignore_errors=True)
                            else:
                                target_path.unlink(missing_ok=True)
                            removed.append(str(target_path))
                        except Exception:
                            pass
            elif browser in ('LibreWolf', 'Firefox'):
                for profile in base.glob('*'):
                    if not profile.is_dir():
                        continue
                    for cache_name in ff_cache_patterns:
                        cache_path = profile / cache_name
                        if cache_path.exists():
                            try:
                                shutil.rmtree(cache_path, ignore_errors=True)
                                removed.append(str(cache_path))
                            except Exception:
                                pass
                    for hist_file in ff_history_files:
                        hist_path = profile / hist_file
                        if hist_path.exists():
                            try:
                                hist_path.unlink(missing_ok=True)
                                removed.append(str(hist_path))
                            except Exception:
                                pass
    messagebox.showinfo("Cleanup", f"Browser cache and search history cleaned for Brave, LibreWolf, and Firefox.\nFiles/folders removed: {len(removed)}")

def delete_junk_files_with_warning():
    answer = messagebox.askyesno("Warning", "This will permanently delete .tmp, .log, .cache, .bak, .dmp, .old, .chk, .gid, .~ files from temp and user folders. Continue?")
    if not answer:
        return
    patterns = [
        str(TEMP_DIR / '**' / '*.tmp'),
        str(TEMP_DIR / '**' / '*.log'),
        str(TEMP_DIR / '**' / '*.cache'),
        str(TEMP_DIR / '**' / '*.bak'),
        str(TEMP_DIR / '**' / '*.dmp'),
        str(TEMP_DIR / '**' / '*.old'),
        str(TEMP_DIR / '**' / '*.chk'),
        str(TEMP_DIR / '**' / '*.gid'),
        str(TEMP_DIR / '**' / '*.~'),
        str(Path.home() / '**' / '*.tmp'),
        str(Path.home() / '**' / '*.log'),
        str(Path.home() / '**' / '*.cache'),
        str(Path.home() / '**' / '*.bak'),
        str(Path.home() / '**' / '*.dmp'),
        str(Path.home() / '**' / '*.old'),
        str(Path.home() / '**' / '*.chk'),
        str(Path.home() / '**' / '*.gid'),
        str(Path.home() / '**' / '*.~'),
    ]
    deleted = 0
    for pattern in patterns:
        for file in glob.glob(pattern, recursive=True):
            try:
                os.remove(file)
                deleted += 1
            except Exception:
                pass
    messagebox.showinfo("Cleanup", f"Deleted {deleted} junk files.")

def clear_temp_and_run_cleanmgr():
    folders = [
        TEMP_DIR,
        Path.home() / 'AppData' / 'Local' / 'Temp',
        Path('C:/Windows/Temp'),
        Path('C:/Windows/Prefetch'),
    ]
    removed = 0
    for folder in folders:
        if folder.exists():
            for item in folder.iterdir():
                try:
                    if item.is_file():
                        item.unlink(missing_ok=True)
                        removed += 1
                    elif item.is_dir():
                        shutil.rmtree(item, ignore_errors=True)
                        removed += 1
                except Exception:
                    pass
    try:
        subprocess.run(['cleanmgr.exe', '/LOWDISK', '/d', 'C:'], check=False)
    except Exception:
        pass
    messagebox.showinfo("Cleanup", f"Temp/cache folders cleaned ({removed} items). Disk Cleanup launched.")

BOOSTER_COMMANDS = {
    "Optimize Network": "Network optimization commands",
    "Optimize System": "System optimization commands",
    "Boost power settings and cpu": "Power and CPU optimization commands",
    "Disable Xbox Stuff": "Xbox feature disabling commands",
    "Debloat System Services": "Service optimization commands",
    "Optimize Settings": "Windows settings optimization commands",
    "Make Everything In list": "All optimization commands"
}
BOOSTER_DEFINITIONS = {
    "Optimize Network": "Optimizes network settings for better performance",
    "Optimize System": "Optimizes system settings for better performance",
    "Boost power settings and cpu": "Optimizes power settings and CPU performance",
    "Disable Xbox Stuff": "Disables Xbox-related features and services",
    "Debloat System Services": "Optimizes and removes unnecessary system services",
    "Optimize Settings": "Optimizes Windows settings for better performance",
    "Make Everything In list": "Executes all optimization options"
}
BOOSTER_FUNCTIONS = {
    "Optimize Network": optimize_network,
    "Optimize System": optimize_general_system,
    "Boost power settings and cpu": optimize_windows_settings,
    "Disable Xbox Stuff": disable_xbox_stuff,
    "Debloat System Services": debloat_system_services,
    "Optimize Settings": optimize_windows_settings,
    "Make Everything In list": make_everything
}
TASKS_COMMANDS = {
    "NumLock at Startup": "Registry command to enable or disable NumLock at startup.",
    "Hidden Files": "Registry command to show or hide hidden files in Explorer.",
    "Make BSOD Better": "Registry commands to improve BSOD info and disable promo screens.",
    "Home and Gallery": "Registry commands to remove or restore Home and Gallery in Explorer.",
    "Hibernation": "System command to enable or disable hibernation.",
    "Reserved Storage": "PowerShell command to enable or disable reserved storage.",
    "Edge Browser": "PowerShell script and registry commands to debloat or restore Microsoft Edge.",
    "Copilot AI": "Registry and PowerShell commands to enable or disable Copilot AI.",
    "Cortana": "Registry and PowerShell commands to enable or disable Cortana.",
    "Nagle Algorithm For Minecraft": "Registry commands to enable or disable Nagle's Algorithm for gaming.",
    "Dark Mode": "Registry commands to enable or disable Windows dark mode.",
    "Ads In Windows": "Registry commands to enable or disable Windows advertising features.",
    "Sticky Keys": "Registry commands to enable or disable Sticky Keys and related features.",
    "BitLocker Encryption": "System commands to enable or disable BitLocker drive encryption.",
    "Set taskbar to left": "Registry and Explorer restart to align taskbar icons left or center.",
}
TASKS_DEFINITIONS = {
    "NumLock at Startup": "Enables or disables NumLock at Windows startup.",
    "Hidden Files": "Shows or hides hidden files in File Explorer.",
    "Make BSOD Better": "Improves BSOD information and disables promo screens.",
    "Home and Gallery": "Removes or restores Home and Gallery from Explorer.",
    "Hibernation": "Enables or disables Windows hibernation feature.",
    "Reserved Storage": "Enables or disables Windows reserved storage.",
    "Edge Browser": "Debloats or restores Microsoft Edge browser.",
    "Copilot AI": "Enables or disables Windows Copilot AI.",
    "Cortana": "Enables or disables Cortana assistant.",
    "Nagle Algorithm For Minecraft": "Toggles Nagle's Algorithm for better gaming ping.",
    "Dark Mode": "Enables or disables Windows dark mode.",
    "Ads In Windows": "Enables or disables advertising features in Windows.",
    "BitLocker Encryption": "Enables or disables BitLocker drive encryption.",
    "Set taskbar to left": "Aligns the taskbar icons to the left or center (Windows 11).",
    "Sticky Keys": "Enables or disables Sticky Keys and related accessibility features."
}

TASKS_FUNCTIONS = {
    "NumLock at Startup": {"enable": enable_numlock, "disable": disable_numlock},
    "Hidden Files": {"enable": show_hidden_files, "disable": hide_hidden_files},
    "Make BSOD Better": {"enable": enable_bsod_parameters, "disable": disable_bsod_parameters},
    "Home and Gallery": {"enable": restore_home_gallery, "disable": remove_home_gallery},
    "Hibernation": {"enable": enable_hibernation, "disable": disable_hibernation},
    "Reserved Storage": {"enable": enable_reserved_storage, "disable": disable_reserved_storage},
    "Edge Browser": {"enable": restore_edge, "disable": debloat_edge},
    "Copilot AI": {"enable": enable_copilot_ai, "disable": disable_copilot_ai},
    "Cortana": {"enable": enable_cortana, "disable": disable_cortana},
    "Nagle Algorithm For Minecraft": {"enable": enable_nagle_algorithm, "disable": disable_nagle_algorithm},
    "Dark Mode": {"enable": enable_windows_dark_mode, "disable": disable_windows_dark_mode},
    "Bing Search in Start Menu": {"enable": enable_bing_search, "disable": disable_bing_search},
    "Classic Right-Click Menu": {"enable": enable_classic_menu, "disable": disable_classic_menu},
    "Ads In Windows": {"enable": enable_ads, "disable": disable_ads},
    "BitLocker Encryption": {"enable": enable_bitlocker, "disable": disable_bitlocker}, 
    "Set taskbar to left": {"enable": enable_taskbar_left, "disable": disable_taskbar_left},
    "Sticky Keys": {"enable": enable_sticky_keys, "disable": disable_sticky_keys}
}
def play_splash_in_gui(root, video_path, on_finish):
    video_file = resource_path(video_path)
    if not os.path.exists(video_file):
        print("[!] Warning: Splash video not found")
        on_finish()
        return
    splash_overlay = ctk.CTkFrame(root, width=720, height=180, corner_radius=0, fg_color="black")
    splash_overlay.place(x=0, y=0, relwidth=1, relheight=1)
    label = ctk.CTkLabel(splash_overlay, text="")
    label.pack(expand=True, fill="both")
    try:
        cap = cv2.VideoCapture(video_file)
        if not cap.isOpened():
            print("[!] Warning: Could not open splash video")
            splash_overlay.destroy()
            on_finish()
            return
    except Exception as e:
        print(f"[!] Error loading splash video: {e}")
        splash_overlay.destroy()
        on_finish()
        return
    def update_frame():
        ret, frame = cap.read()
        if not ret:
            cap.release()
            fade_out()
            return
        frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
        img = Image.fromarray(frame)
        img = img.resize((440, 170), Image.Resampling.LANCZOS)
        imgtk = ImageTk.PhotoImage(image=img)
        label.imgtk = imgtk
        label.configure(image=imgtk)
        splash_overlay.after(25, update_frame)
    def fade_out():
        try:
            splash_overlay.attributes("-alpha", 1.0)
            for alpha in range(10, -1, -1):
                splash_overlay.attributes("-alpha", alpha / 10)
                splash_overlay.update()
                time.sleep(0.05)

        except:
            pass
        splash_overlay.destroy()
        on_finish()
    try:
        splash_overlay.attributes("-alpha", 1.0)
    except:
        pass
    update_frame()
def ensure_resources():
    """Check and download all required resources before launching the UI"""
    required_files = {
        "orange.ico": "https://raw.githubusercontent.com/adasjusk/OrangBooster/beta/files/orange.ico",
        "orange.png": "https://raw.githubusercontent.com/adasjusk/OrangBooster/beta/files/orange.png",
        "brave.png": "https://raw.githubusercontent.com/adasjusk/OrangBooster/beta/files/brave.png",
        "chromium.png": "https://raw.githubusercontent.com/adasjusk/OrangBooster/beta/files/chromium.png",
        "arc.png": "https://raw.githubusercontent.com/adasjusk/OrangBooster/beta/files/arc.png",
        "video.mp4": "https://raw.githubusercontent.com/adasjusk/OrangBooster/beta/files/video.mp4",
        "orange.json": "https://raw.githubusercontent.com/adasjusk/OrangBooster/beta/files/orange.json"
    }
    
    for filename, url in required_files.items():
        file_path = resource_path(filename)
        if not os.path.exists(file_path):
            try:
                print(f"Downloading {filename}...")
                response = requests.get(url, timeout=10)
                response.raise_for_status()
                
                # Ensure directory exists
                os.makedirs(os.path.dirname(file_path) if os.path.dirname(file_path) else '.', exist_ok=True)
                
                # Save the file
                with open(file_path, 'wb') as f:
                    f.write(response.content)
                print(f"Successfully downloaded {filename}")
            except Exception as e:
                print(f"Failed to download {filename}: {e}")
                return False
    
    return True

def load_custom_theme():
    """Load theme configuration from themes.json"""
    try:
        theme_path = resource_path("themes.json")
        if os.path.exists(theme_path):
            with open(theme_path, "r", encoding="utf-8") as f:
                return theme_path
        print("[!] Warning: themes.json not found")
        return None
    except Exception as e:
        print(f"[!] Warning: Theme loading failed: {e}")
        return None

# Ensure the application directory exists before anything else
os.makedirs(APP_DIR, exist_ok=True)
# Download all required resources before any theme or GUI code
if not ensure_resources():
    # Create a hidden Tk root for messagebox
    import tkinter as tk
    root = tk.Tk()
    root.withdraw()
    messagebox.showerror("Error", "Failed to download required resources. Please check your internet connection and try again.")
    sys.exit(1)
# Now set the theme
theme_path = resource_path('orange.json')
ctk.set_default_color_theme(theme_path)

def print_banner():
    print(" [██████╗ ██████╗  █████╗ ███╗   ██╗ ██████╗ ███████╗")
    print("[██╔═══██╗██╔══██╗██╔══██╗████╗  ██║██╔════╝ ██╔════╝")
    print("[██║   ██║██████╔╝███████║██╔██╗ ██║██║  ███╗█████╗  ")
    print("[██║   ██║██╔══██╗██╔══██║██║╚██╗██║██║   ██║██╔══╝  ")
    print("[╚██████╔╝██║  ██║██║  ██║██║ ╚████║╚██████╔╝███████╗")
    print(" [╚═════╝ ╚═╝  ╚═╝╚═╝  ╚═╝╚═╝  ╚═══╝ ╚═════╝ ╚══════╝")

def launch_gui():
    print_banner()
    try:
        print("[*] Initializing program...")
        if not init_program():
            messagebox.showerror("Error", "Failed to initialize program. Please check logs and try again.")
            sys.exit(1)
        print("[+] Program initialized successfully")

        root = ctk.CTk()
        root.resizable(False, False)
        root.geometry("720x800")
        screen_width = root.winfo_screenwidth()
        screen_height = root.winfo_screenheight()
        x = (screen_width - 720) // 2
        y = (screen_height - 800) // 2
        root.geometry(f"720x800+{x}+{y}")
        root.title("Orange Booster")
        ctk.set_appearance_mode("dark")
        theme_path = APP_DIR / 'orange.json'
        if theme_path.exists():
            ctk.set_default_color_theme(str(theme_path))
        else:
            ctk.set_default_color_theme("orange.json")
        app = OrangeBoosterApp(root)
        app.show_admin_warning()
        play_splash_in_gui(root, "video.mp4", lambda: app.show_tab("Info"))
        root.mainloop()
    except Exception as e:
        messagebox.showerror("Critical Error", f"Failed to start application:\n{e}")
        sys.exit(1)

class OrangeBoosterApp:
    def __init__(self, root):
        self.saved_state = self.load_state()
        self.root = root
        self.root.iconbitmap(resource_path("orange.ico"))
        try:
            os.makedirs(APP_DIR, exist_ok=True)
        except Exception as e:
            messagebox.showerror("Error", f"Failed to create directory:\n{APP_DIR}\n{str(e)}")
        content_wrapper = ctk.CTkFrame(root, fg_color="transparent")
        content_wrapper.pack(fill="both", expand=True)
        header_frame = ctk.CTkFrame(content_wrapper, fg_color="transparent")
        header_frame.pack(pady=20, anchor="n")
        try:
            orange_img = Image.open(APP_DIR / "orange.png").resize((64, 64), Image.LANCZOS)
            orange_photo = ctk.CTkImage(light_image=orange_img, dark_image=orange_img, size=(64, 64))
            icon_label = ctk.CTkLabel(header_frame, image=orange_photo, text="")
            icon_label.image = orange_photo
            icon_label.pack(side="left", padx=10)
        except Exception as e:
            print(f"[!] Failed to load logo: {e}")
        title_label = ctk.CTkLabel(header_frame, text="Orange Booster", font=("Comic Sans MS", 28, "bold"))
        title_label.pack(side="left")
        # Ungoogled Chromium is shit
        self.tabs = {
            "Browser": ["Brave Browser", "Ungoogled Chromium", "Arc Browser"],
            "Info": [],
            "Booster": [
                "Optimize Network",
                "Optimize System",
                "Optimize Internet",
                "Disable Xbox Features",
                "Clean System Services",
                "Optimize Settings",
                "Apply All Optimizations"
            ],
            "Tasks": [
                "NumLock at Startup",
                "Hidden Files",
                "Make BSOD Better",
                "Home and Gallery",
                "Hibernation",
                "Reserved Storage",
                "Edge Browser",
                "Copilot AI",
                "Cortana",
                "Nagle Algorithm For Minecraft",
                "Dark Mode",
                "Bing Search in Start Menu",
                "Classic Right-Click Menu",
                "Ads In Windows",
                "BitLocker Encryption",
                "Set taskbar to left",
                "Sticky Keys"
            ],
            "Cleanup": []
        }
        # Setup tab buttons (no background frame)
        self.tab_frame = ctk.CTkFrame(content_wrapper, fg_color="transparent", border_width=0)
        self.tab_frame.pack(pady=5)
        self.tab_buttons = {}
        for idx, name in enumerate(self.tabs):
            btn = ctk.CTkButton(
                self.tab_frame, text=name, width=80,
                fg_color="#333333", text_color="#ffffff",
                command=lambda n=name: self.show_tab(n)
            )
            btn.grid(row=0, column=idx, padx=5)
            self.tab_buttons[name] = btn
        self.options_frame = ctk.CTkFrame(content_wrapper, fg_color="transparent")
        self.options_frame.pack(padx=20, pady=10)
        self.toggle_vars = {}
        self.show_tab("Updates & About")
    def show_tab(self, tab_name):
        for name, btn in self.tab_buttons.items():
            if name == tab_name:
                btn.configure(fg_color="#ff7f00", text_color="#000000")
            else:
                btn.configure(fg_color="#333333", text_color="#ffffff")
        for widget in self.options_frame.winfo_children():
            widget.destroy()
        self.toggle_vars = {}
        if tab_name == "Info":
            sys_info = [
                "OrangBooster v7.0",
                "InterJava Studio",
                "Designed By Vakarux",
                "Coded by adasjusk",
                "",
                "About:",
                f"{platform.system()} {'11' if is_windows_11() else '10'} (Build {platform.version()})"
            ]
            for item in sys_info:
                label = ctk.CTkLabel(self.options_frame, text=item, font=("Helvetica", 13))
                label.pack(pady=0, anchor="center")
            update_button = ctk.CTkButton(self.options_frame, text="Check For Updates", width=120, command=check_for_updates)
            update_button.pack(pady=(4, 4))
            return
        if tab_name == "Booster":
            title = ctk.CTkLabel(self.options_frame, text="Select Boosts to Apply", font=("Arial", 20, "bold"))
            title.pack(pady=10)
            boost_options = [
                ("Optimize Network", optimize_network),
                ("Optimize System", lambda: optimize_general_system(self)),
                ("Optimize Internet", optimize_internet_ping),
                ("Disable Xbox Features", disable_xbox_stuff),
                ("Clean System Services", debloat_system_services),
                ("Optimize Settings", lambda: optimize_windows_settings(self)),
            ]
            self.boost_vars = {}
            for name, func in boost_options:
                var = ctk.BooleanVar()
                chk = ctk.CTkCheckBox(self.options_frame, text=name, variable=var)
                chk.pack(anchor="w", pady=4, padx=10)
                self.boost_vars[name] = (var, func)
            def run_selected_boosts():
                selected = [(name, func) for name, (var, func) in self.boost_vars.items() if var.get()]
                if not selected:
                    messagebox.showinfo("No Boost Selected", "Please select at least one boost to execute.")
                    return
                def run():
                    for name, func in selected:
                        try:
                            func()
                        except Exception as e:
                            print(f"[!] {name} failed: {e}")
                            messagebox.showerror("Error", f"{name} failed:\n{e}")
                    messagebox.showinfo("Done", "Selected boosts executed.")
                threading.Thread(target=run, daemon=True).start()
            ctk.CTkButton(self.options_frame, text="Execute Selected Boosts", command=run_selected_boosts, width=220, height=32).pack(pady=12)
            return
        if tab_name == "Browser":
            self.options_frame.grid_columnconfigure((0, 1, 2), weight=1, uniform="browser")
            browsers = [
                ("Brave Browser", "brave.png", open_brave_browser),
                ("Ungoogled Chromium", "chromium.png", open_ungoogled_chromium),
                ("Arc Browser", "arc.png", open_arc_browser)
            ]
            for idx, (name, img_file, func) in enumerate(browsers):
                try:
                    img_path = os.path.join(APP_DIR, img_file)
                    if not os.path.exists(img_path):
                        ensure_resources()
                    img = Image.open(img_path).resize((96, 96), Image.LANCZOS)
                    photo = ImageTk.PhotoImage(img)
                except Exception as e:
                    print(f"[!] Failed to load {img_file}: {e}")
                    photo = None
                browser_frame = ctk.CTkFrame(self.options_frame, fg_color="transparent")
                browser_frame.grid(row=0, column=idx, padx=10, pady=10, sticky="nsew")
                img_label = ctk.CTkLabel(browser_frame, image=photo, text="", fg_color="transparent")
                img_label.image = photo
                img_label.pack(anchor="center")
                browser_button = ctk.CTkButton(
                    browser_frame, text=name, fg_color="#ff7f00", text_color="#000000", hover_color="#ffa733", width=110, height=28, corner_radius=10, command=func
                )
                browser_button.pack(pady=(8, 4))
        if tab_name == "Tasks":
            win11 = is_windows_11()
            win11_only_options = [
                "Set taskbar to left",
                "Copilot AI",
                "Classic Right-Click Menu",
                "Home and Gallery",
                "BitLocker Encryption"
            ]
            win10_only_options = [
                "Cortana",
                "Bing Search in Start Menu"
            ]
            for option in self.tabs["Tasks"]:
                ui_to_func = {
                    "Copilot AI": "Copilot AI",
                    "Cortana": "Cortana",
                    "Nagle Algorithm For Minecraft": "Nagle Algorithm For Minecraft",
                    "Windows To Dark Mode": "Dark Mode",
                    "Bing Search in Start Menu": "Bing Search in Start Menu",
                    "Classic Right-Click Menu": "Classic Right-Click Menu",
                    "Ads In Windows": "Ads In Windows",
                    "BitLocker Encryption": "BitLocker Encryption",
                    "Taskbar to left on Windows 11": "Set taskbar to left",
                    "Sticky Keys": "Sticky Keys",
                    "NumLock at Startup": "NumLock at Startup",
                    "Hidden Files": "Hidden Files",
                    "Make BSOD Better": "Make BSOD Better",
                    "Home and Gallery": "Home and Gallery",
                    "Hibernation": "Hibernation",
                    "Reserved Storage": "Reserved Storage",
                    "Edge Browser": "Edge Browser"
                }
                is_disabled = (option in win11_only_options and not win11) or (option in win10_only_options and win11)
                func_key = ui_to_func.get(option, option)
                saved = self.saved_state.get("tasks", {}).get(option, False)
                var = ctk.BooleanVar(value=saved)
                def make_toggle_callback(opt, v, func_key):
                    def callback():
                        if func_key not in TASKS_FUNCTIONS:
                            return
                        try:
                            is_disable = opt.lower().startswith("disable ")
                            if v.get():
                                if is_disable and "disable" in TASKS_FUNCTIONS[func_key]:
                                    TASKS_FUNCTIONS[func_key]["disable"]()
                                    messagebox.showinfo("Success", f"{opt} applied.")
                                elif not is_disable and "enable" in TASKS_FUNCTIONS[func_key]:
                                    TASKS_FUNCTIONS[func_key]["enable"]()
                                    messagebox.showinfo("Success", f"{opt} applied.")
                            else:
                                if is_disable and "enable" in TASKS_FUNCTIONS[func_key]:
                                    TASKS_FUNCTIONS[func_key]["enable"]()
                                    messagebox.showinfo("Success", f"{opt} reverted.")
                                elif not is_disable and "disable" in TASKS_FUNCTIONS[func_key]:
                                    TASKS_FUNCTIONS[func_key]["disable"]()
                                    messagebox.showinfo("Success", f"{opt} reverted.")
                            if "tasks" not in self.saved_state:
                                self.saved_state["tasks"] = {}
                            self.saved_state["tasks"][opt] = v.get()
                            self.save_state()
                        except PermissionError:
                            messagebox.showerror("Permission Error", f"{opt} failed. Please run as administrator.")
                        except Exception as e:
                            messagebox.showerror("Error", f"{opt} failed:\n{e}")
                    return callback
                switch = ctk.CTkSwitch(self.options_frame, text=option, variable=var, onvalue=True, offvalue=False,
                    state="disabled" if is_disabled else "normal",
                    fg_color="#2e2e2e" if is_disabled else None,
                    text_color="#666666" if is_disabled else None,
                    command=make_toggle_callback(option, var, func_key))
                switch.pack(anchor="w", pady=8, padx=10)
                self.toggle_vars[option] = var
        if tab_name == "Cleanup":
            title = ctk.CTkLabel(self.options_frame, text="Cleanup Temporary/Junk Files", font=("Arial", 20, "bold"))
            title.pack(pady=10)
            btn1 = ctk.CTkButton(self.options_frame, text="Clean Browser Data", command=clear_browser_cache_and_history, width=120, height=28)
            btn1.pack(pady=4, padx=10)
            btn2 = ctk.CTkButton(self.options_frame, text="Delete Junk Files", command=delete_junk_files_with_warning, width=120, height=28)
            btn2.pack(pady=4, padx=10)
            btn3 = ctk.CTkButton(self.options_frame, text="Run Cleanmgr", command=clear_temp_and_run_cleanmgr, width=120, height=28)
            btn3.pack(pady=4, padx=10)
            return
    def execute_booster_function(self, func):
        def run_function():
            try:
                func()
                print("[+] Operation completed successfully")
            except Exception as e:
                print(f"[!] Operation failed: {e}")
                messagebox.showerror("Error", f"Operation failed:\n{str(e)}")
        thread = threading.Thread(target=run_function, daemon=True)
        thread.start()
    def toggle_task(self, task_name):
        if task_name not in TASKS_FUNCTIONS:
            return
        is_enabled = self.toggle_vars[task_name].get()
        if "tasks" not in self.saved_state:
            self.saved_state["tasks"] = {}
        self.saved_state["tasks"][task_name] = is_enabled
        self.save_state()
        try:
            if is_enabled:
                if "disable" in TASKS_FUNCTIONS[task_name]:
                    TASKS_FUNCTIONS[task_name]["disable"]()
                    print(f"[+] {task_name} disabled")
            else:
                if "enable" in TASKS_FUNCTIONS[task_name]:
                    TASKS_FUNCTIONS[task_name]["enable"]()
                    print(f"[+] {task_name} enabled")
        except Exception as e:
            print(f"[!] Failed to toggle {task_name}: {e}")
            messagebox.showerror("Error", f"Failed to toggle {task_name}:\n{str(e)}")
    def load_state(self):
        try:
            if STATE_FILE.exists():
                with STATE_FILE.open("r", encoding="utf-8") as f:
                    return json.load(f)
            return {"tasks": {}}
        except Exception as e:
            print(f"[!] Could not load saved state: {e}")
            return {"tasks": {}}
    def save_state(self):
        try:
            os.makedirs(APP_DIR, exist_ok=True)
            state_to_save = {"tasks": self.saved_state.get("tasks", {})}
            temp_file = STATE_FILE.with_suffix('.tmp')
            with open(temp_file, "w", encoding="utf-8") as f:
                json.dump(state_to_save, f, indent=2)
            if os.path.exists(STATE_FILE):
                os.replace(STATE_FILE, STATE_FILE.with_suffix('.bak'))
            os.rename(temp_file, STATE_FILE)
            print(f"[+] State saved successfully to {STATE_FILE}")
        except Exception as e:
            print(f"[!] Could not save state: {e}")
            messagebox.showerror("Error", f"Failed to save state:\n{str(e)}")
    def show_admin_warning(self):
        warning_text = (
            "Warning: Administrator Mode Enabled\n\n"
            "This program can modify system settings with admin privileges.\n"
            "Please note:\n"
            "- This is not malware, but may be flagged due to registry changes\n"
            "- The screen may flicker during some operations\n"
            "- Use at your own risk - no warranty is provided\n"
        )
        messagebox.showwarning("Orange Booster Warning", warning_text)
# Don't you even try to change this vakarux!
if __name__ == "__main__":
    launch_gui()
