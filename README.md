## Table of Contents  
1. [Features](#features)  
2. [Technical Implementation](#technical-implementation)  
   - [Page Architecture](#page-architecture)  
   - [Data Storage](#data-storage)  
3. [Linux Setup Guide](#linux-setup-guide)  
   - [.NET MAUI Installation](#installing-net-maui)  
   - [Rinse And Repeat](#rinse-and-repeat)  
   - [GPU Optimization](#gpu-acceleration-setup)  
   - [Android Emulator Setup](#android-emulator-setup)
   - [TroubleShoot](#troubleshoot)  
# Vendor Management App
This app was built to help convention and festival vendors (who tend to be small business owners) keep track of their sales, profit, expenses, and schedule for events. It was also built to help people by having all of their vendor needs tracked within one place on their phone or tablet.

## Features

* Simple, light-weight, and easy to use
* Built with modern development tools:
  - **Rider** / **Visual Studio** as IDE options
  - **.NET MAUI** framework
  - **C#** for business logic
  - **XAML** for UI design
  - **SQLite** for local data storage
* Comprehensive financial tracking:
  - Sales monitoring
  - Profit calculation
  - Expense tracking
* Event management:
  - Schedule organization
  - Event viewer
  - Vendor-specific event tools

## Technical Implementation

### Page Architecture
Each main page has its own view model and XAML design:
- `CashRegister` (View + ViewModel)
- `FinanceBreakdown` (View + ViewModel)
- `VendorEventViewer` (View + ViewModel)
- `VendorEventManager` (View + ViewModel)

The view model (C# file) contains the page logic, while the XAML file handles the visual design.

### Data Storage
The app uses **SQLite** for persistent local storage:
- All database functionality is contained in the `Repository` folder
- Includes:
  - Database connection management
  - Table definitions
  - Data access operations


# Linux Setup Guide

## Installing NET MAUI

You will need to install MAUI a specific way because the package manager for Ubuntu does not contain .NET MAUI at this time of writing.

1. Follow this guide:  
   [.NET MAUI on Linux with Visual Studio Code](https://techcommunity.microsoft.com/blog/educatordeveloperblog/-net-maui-on-linux-with-visual-studio-code/3982195)
2. Get the script:  
   [.NET Install Scripts](https://dotnet.microsoft.com/en-us/download/dotnet/scripts)

> **Note:** You don't need to worry about C# Dev Kit or anything like that. It would be nice to install the linter though.

## Rinse and Repeat

If anything fails to build, run this command and head to the troubleshooting section:

```bash
dotnet clean
rm -rf bin obj
```

## Get the Latest Video Card Support

```bash
sudo add-apt-repository ppa:kisak/kisak-mesa
sudo apt update
sudo apt upgrade
```

# GPU Acceleration Setup
* If you know that your system supports gpu acceleration, then install these packages:
```bash 
    sudo apt install qemu-kvm libvirt-daemon-system libvirt-clients bridge-utils virt-manager
```

## Add User to Group

```bash
sudo usermod -aG kvm $USER
sudo usermod -aG libvirt $USER
newgrp kvm  # Refresh group permissions
```

## Verify KVM is Working

This will check if your system supports GPU acceleration:

```bash
sudo virsh -c qemu:///system list
kvm-ok 
```

> **Note:** If the output comes back negative, your system does not support GPU acceleration.

## Installing the Emulator

### 1. Install JDK 11 (required)
```bash
sudo apt-get install openjdk-11-jdk
```

### 2. Download Android Command Line Tools
```bash
wget https://dl.google.com/android/repository/commandlinetools-linux-9477386_latest.zip
mkdir -p ~/Android/Sdk/cmdline-tools/latest
unzip commandlinetools-linux-*.zip -d cmdline-tools
mv cmdline-tools/cmdline-tools/* ~/Android/Sdk/cmdline-tools/latest
rm -rf commandlinetools-linux-*.zip cmdline-tools
```

### 3. Set Environment Variables
```bash
echo 'export ANDROID_HOME=$HOME/Android/Sdk' >> ~/.bashrc
echo 'export PATH=$PATH:$ANDROID_HOME/cmdline-tools/latest/bin:$ANDROID_HOME/platform-tools' >> ~/.bashrc
source ~/.bashrc
```

### 4. Install Required Android Packages
```bash
sdkmanager --install "platform-tools" "platforms;android-34" "build-tools;34.0.0"
```

## Create the Emulator

```bash
# Install emulator package
~/Android/Sdk/cmdline-tools/latest/bin/sdkmanager "emulator"

# Install a system image (we'll use Android 13 - API 33)
~/Android/Sdk/cmdline-tools/latest/bin/sdkmanager "system-images;android-33;google_apis;x86_64"

# Create a new emulator (Pixel 5 with API 33)
~/Android/Sdk/cmdline-tools/latest/bin/avdmanager create avd -n Pixel_5_API_33 -k "system-images;android-33;google_apis;x86_64" -d pixel_5
```

> **Tip:** To check what emulators you have already, run:  
> `~/Android/Sdk/emulator/emulator -list-avds`

## Configure Emulator

Edit the .ini file:

```bash
nano ~/.android/avd/Pixel_5_API_33.avd/config.ini
```

Add these lines:

```ini
hw.gpu.enabled=yes
hw.gpu.mode=auto
hw.gpu.vulkan.enabled=yes
hw.gltransport=pipe
```

## Launch the Emulator in a Different Shell
> ***Note:** Code below assumes you have followed the GPU optimization section 
```bash
~/Android/Sdk/emulator/emulator -avd Pixel_5_API_33 \
  -gpu host \
  -accel on \
  -no-snapshot \
  -no-audio \
  -no-boot-anim \
  -qemu -m 2048 -enable-kvm
```
> **Note:** Run the command below if you do not have GPU acceleration avialable
```bash 
~/Android/Sdk/emulator/emulator -avd Pixel_5_API_33 \
  -gpu swiftshader_indirect \
  -no-audio \
  -no-boot-anim
```

## Launch the Program

```bash
dotnet build -f net8.0-android -t:Run -p:AndroidSdkDirectory=$HOME/Android/Sdk -p:AndroidDevice=-d
```

## TroubleShooting




  
  
  
