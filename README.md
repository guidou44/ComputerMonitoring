# ComputerResourcesMonitoring
![CI Status](https://github.com/guidou44/ComputerMonitoring/workflows/ComputerResourceMonitoring%20CI/badge.svg?branch=master)
[![codecov](https://codecov.io/gh/guidou44/ComputerMonitoring/branch/master/graph/badge.svg)](https://codecov.io/gh/guidou44/ComputerMonitoring)

## Usage

### EmailReports and PackageCapture

- Only works with windows
- Needs to be run as Administrator because of the higher privileges needed to access MSAcpi_ThermalZone
- you need to add a ReporterConfiguration.xml file with your emails and credentials in directory [Configuration](ComputerRessourcesMonitoring\Configuration). (Otherwise, email notification for packet exchange wont work). It should look like this:

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

- If you want packet capture to work, you need to have [WinPcap](https://www.winpcap.org/install/) installed on computer because pcap uses its dll, otherwise, it will just not monitor packet exchange.

### GPU

- For GPU monitoring, there is 2 types of connectors possible: 
1. For NVIDIA only GPUs, there is the *NVIDIA_Connector* that uses this [NvAPIWrapper](https://github.com/falahati/NvAPIWrapper). To set this connector for a specific
resource, in the config file [MonitoringConfiguration.cfg](ComputerRessourcesMonitoring\Configuration\MonitoringConfiguration.cfg), specify the connector :
```xml
<ComputerRessource>
	<TargetName>GPU_Load</TargetName>
	<Connector>NVDIA_API</Connector>
	<IsRemote>false</IsRemote>
</ComputerRessource>
```

2. Otherwise, for all GPUs, there is the *OpenHardware_Connector*. It also works with NVIDIA GPUs. It uses [OpenHardwareMonitor](https://github.com/openhardwaremonitor/openhardwaremonitor),
downloaded with Nuget. To set this connector for a GPU resource :
```xml
<ComputerRessource>
	<TargetName>GPU_Clock</TargetName>
	<Connector>OpenHardware</Connector>
	<IsRemote>false</IsRemote>
</ComputerRessource>
```

### Initial targets

- You can set the initial resources to monitor in the config file [MonitoringConfiguration.cfg](ComputerRessourcesMonitoring\Configuration\MonitoringConfiguration.cfg), at the top:
```xml
<InitialTarget>CPU_Load</InitialTarget>
<InitialTarget>GPU_Temp</InitialTarget>
<InitialTarget>Server_CPU_Load</InitialTarget>
```
The maximum is 7 targets.

## Tests

Run the script [Run_Tests](ComputerRessourcesMonitoring\Run_Tests.bat) in folder [ComputerRessourcesMonitoring](ComputerRessourcesMonitoring)

## Code coverage

see on [CodeCov](https://codecov.io/gh/guidou44/ComputerMonitoring)

<sub><sup>(I know about the typo in resource in projects names)</sup></sub>
