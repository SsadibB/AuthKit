#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
AuthKit Setup & Credential Configurator
=======================================
Interactive tool to configure authentication credentials for any Unity project
using AuthKit (PlayFab, Google, Facebook & Instagram).

Usage:
  python configure_auth.py
  (or double-click configure_auth.bat on Windows)
"""

import os
import sys
import re
import uuid

# ANSI Color formatting (works in Windows Terminal, VS Code, PowerShell & macOS/Linux)
class Color:
    HEADER = '\033[95m'
    BLUE = '\033[94m'
    CYAN = '\033[96m'
    GREEN = '\033[92m'
    YELLOW = '\033[93m'
    RED = '\033[91m'
    BOLD = '\033[1m'
    DIM = '\033[2m'
    RESET = '\033[0m'

def print_header(text):
    print(f"\n{Color.CYAN}{Color.BOLD}{'=' * 65}")
    print(f" {text}")
    print(f"{'=' * 65}{Color.RESET}")

def print_step(num, title):
    print(f"\n{Color.BLUE}{Color.BOLD}[Step {num}] {title}{Color.RESET}")

def print_success(msg):
    print(f"  {Color.GREEN}✔ {msg}{Color.RESET}")

def print_info(msg):
    print(f"  {Color.CYAN}ℹ {msg}{Color.RESET}")

def print_warn(msg):
    print(f"  {Color.YELLOW}⚠ {msg}{Color.RESET}")

def prompt_input(prompt_text, default="", required=False, example=""):
    suffix = f" [{Color.DIM}default: {default}{Color.RESET}]" if default else ""
    ex_suffix = f" {Color.DIM}(e.g. {example}){Color.RESET}" if example else ""
    while True:
        try:
            val = input(f"{Color.BOLD}{prompt_text}{suffix}{ex_suffix}:{Color.RESET} ").strip()
        except (KeyboardInterrupt, EOFError):
            print(f"\n{Color.YELLOW}Configuration aborted.{Color.RESET}")
            sys.exit(0)
            
        if not val and default:
            return default
        if not val and required:
            print(f"  {Color.RED}This field is required. Please enter a value.{Color.RESET}")
            continue
        return val

def find_file_recursive(root_dir, filename):
    for root, _, files in os.walk(root_dir):
        if filename in files:
            return os.path.join(root, filename)
    return None

def find_dir_recursive(root_dir, dirname):
    for root, dirs, _ in os.walk(root_dir):
        if dirname in dirs:
            return os.path.join(root, dirname)
    return None

def get_auth_settings_guid(project_dir):
    """Finds the AuthSettings.cs.meta file to retrieve the actual script GUID."""
    meta_path = find_file_recursive(project_dir, "AuthSettings.cs.meta")
    if meta_path and os.path.exists(meta_path):
        try:
            with open(meta_path, "r", encoding="utf-8") as f:
                content = f.read()
                m = re.search(r"guid:\s*([a-fA-F0-9]{32})", content)
                if m:
                    return m.group(1)
        except Exception:
            pass
    # Fallback to the known GUID in AuthKit
    return "4c8e1a2b3d5f47a89b0c1d2e3f4a5b6c"

def write_auth_settings_asset(project_dir, playfab_id, google_id, fb_app_id, fb_client_token):
    """Writes Assets/Resources/AuthSettings.asset with the given credentials."""
    resources_dir = os.path.join(project_dir, "Assets", "Resources")
    os.makedirs(resources_dir, exist_ok=True)
    
    asset_path = os.path.join(resources_dir, "AuthSettings.asset")
    script_guid = get_auth_settings_guid(project_dir)
    
    yaml_content = f"""%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 0}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {script_guid}, type: 3}}
  m_Name: AuthSettings
  m_EditorClassIdentifier: SadibTools.AuthLogin::AuthSettings
  playfabTitleId: {playfab_id}
  googleWebClientId: {google_id}
  facebookAppId: {fb_app_id}
  facebookClientToken: {fb_client_token}
"""
    with open(asset_path, "w", encoding="utf-8") as f:
        f.write(yaml_content)
        
    meta_path = asset_path + ".meta"
    if not os.path.exists(meta_path):
        asset_guid = uuid.uuid4().hex
        meta_content = f"""fileFormatVersion: 2
guid: {asset_guid}
NativeFormatImporter:
  externalObjects: {{}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""
        with open(meta_path, "w", encoding="utf-8") as f:
            f.write(meta_content)
            
    return asset_path

def update_playfab_shared_settings(project_dir, playfab_id):
    """If PlayFab SDK is present, updates TitleId in PlayFabSharedSettings.asset."""
    asset_path = find_file_recursive(project_dir, "PlayFabSharedSettings.asset")
    if not asset_path or not os.path.exists(asset_path):
        return None
        
    try:
        with open(asset_path, "r", encoding="utf-8") as f:
            lines = f.readlines()
            
        updated = False
        new_lines = []
        for line in lines:
            if line.strip().startswith("TitleId:"):
                new_lines.append(f"  TitleId: {playfab_id}\n")
                updated = True
            else:
                new_lines.append(line)
                
        if updated:
            with open(asset_path, "w", encoding="utf-8") as f:
                f.writelines(new_lines)
            return asset_path
    except Exception as e:
        print_warn(f"Could not update PlayFabSharedSettings.asset: {e}")
    return None

def write_android_strings_xml(project_dir, fb_app_id, fb_client_token):
    """Writes res/values/strings.xml inside AuthKitGoogleSignIn.androidlib."""
    lib_dir = find_dir_recursive(project_dir, "AuthKitGoogleSignIn.androidlib")
    if not lib_dir:
        return None
        
    values_dir = os.path.join(lib_dir, "res", "values")
    os.makedirs(values_dir, exist_ok=True)
    
    app_id = fb_app_id if fb_app_id else "0"
    client_tok = fb_client_token if fb_client_token else "placeholder"
    scheme = f"fb{app_id}"
    
    xml_content = f"""<?xml version="1.0" encoding="utf-8"?>
<resources>
    <string name="facebook_app_id">{app_id}</string>
    <string name="facebook_client_token">{client_tok}</string>
    <string name="fb_login_protocol_scheme">{scheme}</string>
</resources>
"""
    strings_path = os.path.join(values_dir, "strings.xml")
    with open(strings_path, "w", encoding="utf-8") as f:
        f.write(xml_content)
    return strings_path

def update_bundle_id_project_settings(project_dir, bundle_id):
    """Updates the Android bundle identifier in ProjectSettings/ProjectSettings.asset if present."""
    proj_settings = os.path.join(project_dir, "ProjectSettings", "ProjectSettings.asset")
    if not os.path.exists(proj_settings):
        return None
        
    try:
        with open(proj_settings, "r", encoding="utf-8") as f:
            content = f.read()
            
        # Check for applicationIdentifier section
        if "applicationIdentifier:" in content:
            # Replace Android bundle id if existing
            if re.search(r"Android:\s*[^\n]+", content):
                new_content = re.sub(r"(applicationIdentifier:[\s\S]*?Android:\s*)([^\n]+)", rf"\g<1>{bundle_id}", content)
            else:
                new_content = content.replace("applicationIdentifier:", f"applicationIdentifier:\n    Android: {bundle_id}")
            with open(proj_settings, "w", encoding="utf-8") as f:
                f.write(new_content)
            return proj_settings
    except Exception as e:
        print_warn(f"Could not update ProjectSettings.asset: {e}")
    return None

def main():
    print_header("AuthKit Project Credential Configurator")
    print(f" {Color.DIM}Automatically sets up PlayFab, Google, Facebook & Instagram for your game{Color.RESET}")

    # 1. Project Directory Detection
    current_dir = os.path.abspath(os.getcwd())
    is_unity_project = os.path.exists(os.path.join(current_dir, "Assets"))
    
    print_step(1, "Select Target Unity Project")
    if is_unity_project:
        print_info(f"Detected current directory as Unity project: {current_dir}")
        proj_dir_input = prompt_input("Use this Unity project path? (Press Enter to confirm or type path)", default=current_dir)
    else:
        proj_dir_input = prompt_input("Enter the path to your Unity project root folder", required=True)
        
    project_dir = os.path.abspath(proj_dir_input)
    if not os.path.exists(os.path.join(project_dir, "Assets")):
        print(f"\n{Color.RED}Error: '{project_dir}' does not appear to be a valid Unity project (no 'Assets' folder found).{Color.RESET}")
        sys.exit(1)
        
    print_success(f"Target project: {project_dir}")

    # 2. Gather Credentials Step-by-Step
    print_step(2, "Enter Authentication Credentials")
    print(f"  {Color.DIM}(Tip: Leave optional items blank or press Enter to skip if not using that provider){Color.RESET}\n")

    # PlayFab Title ID
    playfab_id = prompt_input(
        "Put your PlayFab Title ID",
        required=True,
        example="10B581 or AB12C"
    )

    # Android Bundle ID
    bundle_id = prompt_input(
        "Put your Android Bundle ID / Package Name",
        example="com.yourcompany.sudoku"
    )

    # Google Web Client ID
    google_web_id = prompt_input(
        "Put your Google OAuth Web Client ID (Google Cloud Console)",
        example="1234567890-abcdef.apps.googleusercontent.com"
    )

    # Facebook App ID
    fb_app_id = prompt_input(
        "Put your Facebook / Instagram App ID (Meta Developer Portal)",
        example="123456789012345"
    )

    # Facebook Client Token
    fb_client_token = ""
    if fb_app_id:
        fb_client_token = prompt_input(
            "Put your Facebook Client Token (Meta Portal -> App Settings -> Advanced)",
            example="a1b2c3d4e5f6g7h8..."
        )

    # 3. Confirmation
    print_step(3, "Confirmation & Ingestion")
    print(f"  • PlayFab Title ID    : {Color.BOLD}{playfab_id}{Color.RESET}")
    print(f"  • Android Bundle ID   : {Color.BOLD}{bundle_id or '(Not specified)'}{Color.RESET}")
    print(f"  • Google Web Client ID: {Color.BOLD}{google_web_id or '(Not specified)'}{Color.RESET}")
    print(f"  • Facebook App ID     : {Color.BOLD}{fb_app_id or '(Not specified)'}{Color.RESET}")
    print(f"  • Facebook Client Token: {Color.BOLD}{('******' if fb_client_token else '(Not specified)')}{Color.RESET}")

    proceed = prompt_input("\nApply these settings to your project now? (Y/n)", default="Y").lower()
    if proceed not in ["y", "yes"]:
        print(f"\n{Color.YELLOW}Aborted. No changes were made.{Color.RESET}")
        sys.exit(0)

    # 4. Inject into files
    print_step(4, "Injecting Credentials into Project Files")
    
    # 4a. AuthSettings.asset
    auth_asset = write_auth_settings_asset(project_dir, playfab_id, google_web_id, fb_app_id, fb_client_token)
    print_success(f"Wrote settings: {auth_asset}")
    
    # 4b. PlayFabSharedSettings.asset
    pf_asset = update_playfab_shared_settings(project_dir, playfab_id)
    if pf_asset:
        print_success(f"Updated PlayFab Title ID in: {pf_asset}")
    else:
        print_info("PlayFabSharedSettings.asset not found (will be read from AuthSettings at runtime).")

    # 4c. Android strings.xml (Facebook Scheme)
    if fb_app_id:
        strings_xml = write_android_strings_xml(project_dir, fb_app_id, fb_client_token)
        if strings_xml:
            print_success(f"Wrote Facebook Android strings to: {strings_xml}")
        else:
            print_info("Android library strings will be auto-generated by Unity during Android build.")

    # 4d. ProjectSettings Bundle ID
    if bundle_id:
        proj_set = update_bundle_id_project_settings(project_dir, bundle_id)
        if proj_set:
            print_success(f"Updated Android Bundle Identifier in ProjectSettings: {proj_set}")

    # 5. Done!
    print_header("Configuration Completed Successfully! 🎉")
    print(f"""
{Color.GREEN}All authentication credentials have been successfully injected.{Color.RESET}

Next Steps in Unity:
  1. Open your Unity project.
  2. Wire up your UI buttons to:
       - Google:    AuthManager.Instance.SignInWithGoogle()
       - Facebook:  AuthManager.Instance.SignInWithFacebook()
       - Instagram: AuthManager.Instance.SignInWithInstagram()
  3. Build and test on your Android device!
""")

if __name__ == "__main__":
    main()