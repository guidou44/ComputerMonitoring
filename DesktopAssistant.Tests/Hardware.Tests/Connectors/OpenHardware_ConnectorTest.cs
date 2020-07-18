using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hardware.Connectors;
using Hardware.Enums;
using Hardware.Components;
using Hardware.Wrappers;
using Moq;
using Xunit;

namespace DesktopAssistantTests.HardwareAccessTests.Connectors
{
    public class OpenHardware_ConnectorTest : ConnectorBaseTest
    {
        private const float FAN_SPEED = 300.0f; //cast to double
        private const string MOTHERBOARD_MAKE = "TEST_MOTHERBOARD";
        private const string GPU_MAKE = "N_V_I_D_I_A";

        private const float CLOCK_SENSOR_VALUE = 1.0f;
        private const float MEM_CONTROLLER_SENSOR_VALUE = 2.0f;
        private const float MEM_CLOCK_SENSOR_VALUE = 3.0f;
        private const float MEM_LOAD_SENSOR_VALUE = 4.0f;
        private const float SHADER_CLOCK_SENSOR_VALUE = 5.0f;
        private const float TEMP_SENSOR_VALUE = 6.0f;
        private const float LOAD_SENSOR_VALUE = 77.0f;
        private const float VIDEO_ENG_LOAD_SENSOR_VALUE = 8.0f;

        [Fact]
        public void GivenWrapper_WhenInstantiateConnector_ThenItCallsWrapperOpen()
        {
            Mock<IOpenHardwareComputer> wrapper = new Mock<IOpenHardwareComputer>();
            wrapper.Setup(c => c.Open()).Verifiable();
            OpenHardware_Connector connector = new OpenHardware_Connector(wrapper.Object);

            wrapper.Verify(c => c.Open(), Times.Once);
        }

        protected override KeyValuePair<ConnectorBase, IDictionary<MonitoringTarget, object>> ProvideConnectorTargetsAndExpected()
        {
            OpenHardware_Connector connector = GetConnector();
            IDictionary<MonitoringTarget, object> targetsAndExpected = new Dictionary<MonitoringTarget, object>() 
            {
                { MonitoringTarget.GPU_Clock, Math.Round((double)CLOCK_SENSOR_VALUE, 2)},
                { MonitoringTarget.GPU_Memory_Controller, Math.Round((double)MEM_CONTROLLER_SENSOR_VALUE, 2)},
                { MonitoringTarget.GPU_Memory_Clock, Math.Round((double)MEM_CLOCK_SENSOR_VALUE, 2)},
                { MonitoringTarget.GPU_Memory_Load, Math.Round((double)MEM_LOAD_SENSOR_VALUE, 2)},
                { MonitoringTarget.GPU_Shader_Clock, Math.Round((double)SHADER_CLOCK_SENSOR_VALUE, 2)},
                { MonitoringTarget.GPU_Temp, Math.Round((double)TEMP_SENSOR_VALUE, 2)},
                { MonitoringTarget.GPU_Load, Math.Round((double)LOAD_SENSOR_VALUE, 2)},
                { MonitoringTarget.GPU_VideoEngine_Load, Math.Round((double)VIDEO_ENG_LOAD_SENSOR_VALUE, 2)},
                { MonitoringTarget.GPU_Make, GPU_MAKE},
                { MonitoringTarget.Mother_Board_Make, MOTHERBOARD_MAKE},
                { MonitoringTarget.FAN_Speed, (double) FAN_SPEED },
            };

            return new KeyValuePair<ConnectorBase, IDictionary<MonitoringTarget, object>>(connector, targetsAndExpected);
        }

        protected override KeyValuePair<ConnectorBase, MonitoringTarget> ProvideConnectorWithTargetThatThrows()
        {
            OpenHardware_Connector connector = GetConnector();
            return new KeyValuePair<ConnectorBase, MonitoringTarget>(connector, MonitoringTarget.CPU_Make);
        }

        public OpenHardware_Connector GetConnector()
        {
            Mock<IOpenHardwareComputer> computer = new Mock<IOpenHardwareComputer>();

            Mock<IOpenHardware> gpu = new Mock<IOpenHardware>();
            Mock<IOpenHardware> motherboard = new Mock<IOpenHardware>();
            Mock<IOpenHardware> fan = new Mock<IOpenHardware>();

            Mock<IOpenSensor> fanSensor = new Mock<IOpenSensor>();

            Mock<IOpenSensor> gpuClockSensor = new Mock<IOpenSensor>();
            Mock<IOpenSensor> gpuMemoryControllerSensor = new Mock<IOpenSensor>();
            Mock<IOpenSensor> gpuMemoryClockSensor = new Mock<IOpenSensor>();            
            Mock<IOpenSensor> gpuMemoryLoadSensor = new Mock<IOpenSensor>();
            Mock<IOpenSensor> gpuShaderClockSensor = new Mock<IOpenSensor>();
            Mock<IOpenSensor> gpuTempSensor = new Mock<IOpenSensor>();
            Mock<IOpenSensor> gpuLoadSensor = new Mock<IOpenSensor>();
            Mock<IOpenSensor> gpuVideoEngineLoadSensor = new Mock<IOpenSensor>();

            gpuClockSensor.SetupGet(g => g.SensorType).Returns(OpenHardwareSensorType.Clock);
            gpuMemoryControllerSensor.SetupGet(g => g.SensorType).Returns(OpenHardwareSensorType.Load);
            gpuMemoryClockSensor.SetupGet(g => g.SensorType).Returns(OpenHardwareSensorType.Clock);
            gpuMemoryLoadSensor.SetupGet(g => g.SensorType).Returns(OpenHardwareSensorType.Load);
            gpuShaderClockSensor.SetupGet(g => g.SensorType).Returns(OpenHardwareSensorType.Clock);
            gpuTempSensor.SetupGet(g => g.SensorType).Returns(OpenHardwareSensorType.Temperature);
            gpuLoadSensor.SetupGet(g => g.SensorType).Returns(OpenHardwareSensorType.Load);
            gpuVideoEngineLoadSensor.SetupGet(g => g.SensorType).Returns(OpenHardwareSensorType.Load);

            gpuClockSensor.SetupGet(g => g.Name).Returns("GPU Core");
            gpuMemoryControllerSensor.SetupGet(g => g.Name).Returns("GPU Memory Controller");
            gpuMemoryClockSensor.SetupGet(g => g.Name).Returns("GPU Memory");
            gpuMemoryLoadSensor.SetupGet(g => g.Name).Returns("GPU Memory");
            gpuShaderClockSensor.SetupGet(g => g.Name).Returns("GPU Shader");
            gpuTempSensor.SetupGet(g => g.Name).Returns("GPU Core");
            gpuLoadSensor.SetupGet(g => g.Name).Returns("GPU Core");
            gpuVideoEngineLoadSensor.SetupGet(g => g.Name).Returns("GPU Video Engine");

            gpuClockSensor.SetupGet(g => g.Value).Returns(CLOCK_SENSOR_VALUE);
            gpuMemoryControllerSensor.SetupGet(g => g.Value).Returns(MEM_CONTROLLER_SENSOR_VALUE);
            gpuMemoryClockSensor.SetupGet(g => g.Value).Returns(MEM_CLOCK_SENSOR_VALUE);
            gpuMemoryLoadSensor.SetupGet(g => g.Value).Returns(MEM_LOAD_SENSOR_VALUE);
            gpuShaderClockSensor.SetupGet(g => g.Value).Returns(SHADER_CLOCK_SENSOR_VALUE);
            gpuTempSensor.SetupGet(g => g.Value).Returns(TEMP_SENSOR_VALUE);
            gpuLoadSensor.SetupGet(g => g.Value).Returns(LOAD_SENSOR_VALUE);
            gpuVideoEngineLoadSensor.SetupGet(g => g.Value).Returns(VIDEO_ENG_LOAD_SENSOR_VALUE);

            fanSensor.SetupGet(sf => sf.SensorType).Returns(OpenHardwareSensorType.Fan);
            fanSensor.SetupGet(sf => sf.Name).Returns("Fan #1");
            fanSensor.SetupGet(sf => sf.Value).Returns(FAN_SPEED);
            fan.SetupGet(f => f.Sensors).Returns(new IOpenSensor[] { fanSensor.Object });

            gpu.SetupGet(g => g.HardwareType).Returns(OpenHardwareType.GpuNvidia);
            gpu.SetupGet(g => g.Name).Returns(GPU_MAKE);
            gpu.SetupGet(g => g.Sensors).Returns(new IOpenSensor[] 
            { 
                gpuClockSensor.Object, 
                gpuMemoryControllerSensor.Object, 
                gpuMemoryClockSensor.Object,
                gpuMemoryLoadSensor.Object,
                gpuShaderClockSensor.Object,
                gpuTempSensor.Object,
                gpuLoadSensor.Object,
                gpuVideoEngineLoadSensor.Object
            });

            gpu.Setup(g => g.Update()).Verifiable();
            motherboard.Setup(m => m.Update()).Verifiable();
            fan.Setup(f => f.Update()).Verifiable();

            motherboard.SetupGet(m => m.Name).Returns(MOTHERBOARD_MAKE);
            motherboard.SetupGet(m => m.HardwareType).Returns(OpenHardwareType.Mainboard);

            motherboard.SetupGet(m => m.SubHardware).Returns(new IOpenHardware[] { fan.Object });
            gpu.SetupGet(g => g.SubHardware).Returns(new IOpenHardware[0]);          

            computer.Setup(c => c.Open()).Verifiable();
            computer.SetupGet(c => c.Hardware).Returns(new IOpenHardware[] { gpu.Object, motherboard.Object });

            return new OpenHardware_Connector(computer.Object);
        }
    }
}
