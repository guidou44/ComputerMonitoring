- Only work with windows
- Needs to be run as Administrator because of the higher privileges needed to access MSAcpi_ThermalZone
- you need to add a ReporterConfiguration.xml file with your emails and credentials: It should look like this:

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

- if you want packet capture to work, you need to have winpcap installed on computer because pcap uses its dll