# DesktopAssistant
![CI Status](https://github.com/guidou44/ComputerMonitoring/workflows/DesktopAssistant%20CI/badge.svg?branch=master)
[![codecov](https://codecov.io/gh/guidou44/ComputerMonitoring/branch/master/graph/badge.svg)](https://codecov.io/gh/guidou44/ComputerMonitoring)

## Usage

### EmailReports

- Only works with windows, well it's .NET Framework!
- Needs to be run as Administrator because of the higher privileges needed to access MSAcpi_ThermalZone
- you need to add a ReporterConfiguration.xml file with your emails and credentials in directory [Configuration](ComputerRessourcesMonitoring\Configuration) if you want email notifications. (Otherwise, email notification for packet exchange wont work). It should look like this:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
	<Email>
		<Sender>
			<Id>YOUR.SENDER.EMAIL@gmail.com</Id>
			<Password>PASSWORD</Password>
		</Sender>
		<Receivers>
			<Target>
				<Address>YOUR.TARGET.EMAIL@hotmail.com</Address>
			</Target>
		</Receivers>
	</Email>
</configuration>
```

- If you want packet capture to work, you need to have [WinPcap](https://www.winpcap.org/install/) installed on computer because [SharpPcap](https://github.com/chmorgan/sharppcap) uses its dll, otherwise, it will just NOT monitor packet exchange.

### Packets capture

Usage documentation coming soon.

### Hardware

Hardware monitors are managed by the [HardwareManager](https://github.com/guidou44/ComputerMonitoring/blob/master/Hardware/HardwareManager.cs). It uses connectors to access resources. Connectors each have accessible MonitoringTargets.

### GPU

- For GPU monitoring, there is 2 types of connectors possible: 
1. For NVIDIA only GPUs, there is the *NVIDIA_Connector* that uses this [NvAPIWrapper](https://github.com/falahati/NvAPIWrapper). To set this connector for a specific
resource, in the config file [MonitoringConfiguration.cfg](https://github.com/guidou44/ComputerMonitoring/blob/master/DesktopAssistant/Configuration/MonitoringConfiguration.cfg), specify the connector :
```xml
<ComputerRessource>
	<TargetName>GPU_Load</TargetName>
	<Connector>NVDIA_API</Connector>
</ComputerRessource>
```

Thix NvAPIWrapper works well but has a limit to what it can extract from the GPU.

2. Otherwise, for all GPUs, there is the *OpenHardware_Connector*. It also works with NVIDIA GPUs. It uses [OpenHardwareMonitor](https://github.com/openhardwaremonitor/openhardwaremonitor).
To set this connector for a GPU resource :
```xml
<ComputerRessource>
	<TargetName>GPU_Clock</TargetName>
	<Connector>OpenHardware</Connector>
</ComputerRessource>
```

This connector is more generic and has proved to work on more computers. This connector is recommended.

### Initial targets

- You can set the initial resources to monitor in the config file [MonitoringConfiguration.cfg](https://github.com/guidou44/ComputerMonitoring/blob/master/DesktopAssistant/Configuration/MonitoringConfiguration.cfg), at the top:
```xml
<InitialTarget>CPU_Load</InitialTarget>
<InitialTarget>GPU_Temp</InitialTarget>
<InitialTarget>RAM_Usage</InitialTarget>
```
The maximum is 7 targets.

## Tests

Run the script [Run_Tests](https://github.com/guidou44/ComputerMonitoring/blob/master/DesktopAssistant/Run_Tests.bat) in folder [DesktopAssistant](https://github.com/guidou44/ComputerMonitoring/tree/master/DesktopAssistant)

## Code coverage

see on [CodeCov](https://codecov.io/gh/guidou44/ComputerMonitoring)
