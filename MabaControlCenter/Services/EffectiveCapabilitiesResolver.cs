using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public class EffectiveCapabilitiesResolver : IEffectiveCapabilitiesResolver
{
    public CapabilitiesSection Resolve(MachineDefinition definition, CncDriverCapabilities driverCapabilities, DriverType driverType)
    {
        var driver = ToContractCapabilities(driverCapabilities, driverType);
        var machine = definition.Capabilities;

        return new CapabilitiesSection
        {
            Motion = new MotionCapabilities
            {
                Homing = machine.Motion.Homing && driver.Motion.Homing,
                ZHoming = machine.Motion.ZHoming && driver.Motion.ZHoming,
                CombinedXYHoming = machine.Motion.CombinedXYHoming && driver.Motion.CombinedXYHoming,
                RelativeMoves = machine.Motion.RelativeMoves && driver.Motion.RelativeMoves,
                AbsoluteMoves = machine.Motion.AbsoluteMoves && driver.Motion.AbsoluteMoves,
                Pause = machine.Motion.Pause && driver.Motion.Pause,
                Resume = machine.Motion.Resume && driver.Motion.Resume,
                Stop = machine.Motion.Stop && driver.Motion.Stop,
                Park = machine.Motion.Park && driver.Motion.Park,
                CenterMove = machine.Motion.CenterMove && driver.Motion.CenterMove,
                WorkOffset = machine.Motion.WorkOffset && driver.Motion.WorkOffset,
                JogContinuous = machine.Motion.JogContinuous && driver.Motion.JogContinuous,
                JogStep = machine.Motion.JogStep && driver.Motion.JogStep
            },
            Execution = new ExecutionCapabilities
            {
                RealExecution = machine.Execution.RealExecution && driver.Execution.RealExecution,
                Simulation = machine.Execution.Simulation && driver.Execution.Simulation,
                PreviewPlayback = machine.Execution.PreviewPlayback && driver.Execution.PreviewPlayback,
                DryRun = machine.Execution.DryRun && driver.Execution.DryRun,
                FileRun = machine.Execution.FileRun && driver.Execution.FileRun,
                Frame = machine.Execution.Frame && driver.Execution.Frame,
                BoundingBoxPreview = machine.Execution.BoundingBoxPreview && driver.Execution.BoundingBoxPreview,
                LiveReportedPosition = machine.Execution.LiveReportedPosition && driver.Execution.LiveReportedPosition,
                EstimatedPositionOnly = machine.Execution.EstimatedPositionOnly && driver.Execution.EstimatedPositionOnly,
                ToolpathPreview = machine.Execution.ToolpathPreview && driver.Execution.ToolpathPreview,
                ProgressTracking = machine.Execution.ProgressTracking && driver.Execution.ProgressTracking
            },
            Protocol = new ProtocolCapabilities
            {
                Handshake = machine.Protocol.Handshake && driver.Protocol.Handshake,
                Acknowledgements = machine.Protocol.Acknowledgements && driver.Protocol.Acknowledgements,
                AlarmReporting = machine.Protocol.AlarmReporting && driver.Protocol.AlarmReporting,
                AlarmReset = machine.Protocol.AlarmReset && driver.Protocol.AlarmReset,
                StatusQuery = machine.Protocol.StatusQuery && driver.Protocol.StatusQuery,
                PositionQuery = machine.Protocol.PositionQuery && driver.Protocol.PositionQuery,
                MotorEnable = machine.Protocol.MotorEnable && driver.Protocol.MotorEnable,
                MotorDisable = machine.Protocol.MotorDisable && driver.Protocol.MotorDisable,
                FeedHold = machine.Protocol.FeedHold && driver.Protocol.FeedHold,
                SoftReset = machine.Protocol.SoftReset && driver.Protocol.SoftReset
            },
            Visualization = new VisualizationCapabilities
            {
                MachineVisualization = machine.Visualization.MachineVisualization && driver.Visualization.MachineVisualization,
                TopView2D = machine.Visualization.TopView2D && driver.Visualization.TopView2D,
                Perspective3D = machine.Visualization.Perspective3D && driver.Visualization.Perspective3D,
                KinematicsAnimation = machine.Visualization.KinematicsAnimation && driver.Visualization.KinematicsAnimation,
                RealTimePositionDisplay = machine.Visualization.RealTimePositionDisplay && driver.Visualization.RealTimePositionDisplay
            },
            FileHandling = new FileHandlingCapabilities
            {
                LocalFileRun = machine.FileHandling.LocalFileRun && driver.FileHandling.LocalFileRun,
                StreamingExecution = machine.FileHandling.StreamingExecution && driver.FileHandling.StreamingExecution,
                GcodeValidation = machine.FileHandling.GcodeValidation && driver.FileHandling.GcodeValidation,
                MultipleFileFormats = machine.FileHandling.MultipleFileFormats && driver.FileHandling.MultipleFileFormats
            }
        };
    }

    public CncDriverCapabilities ToDriverCapabilities(DriverType driverType)
    {
        return driverType == DriverType.Simulated
            ? new CncDriverCapabilities
            {
                SupportsAcknowledgements = true,
                SupportsAlarmReset = true,
                SupportsCombinedXyMove = false,
                SupportsLivePositionReporting = true,
                SupportsPause = true,
                SupportsWorkCoordinateSystem = true,
                SupportsZHoming = false,
                VisualizationModelType = "Gantry3Axis"
            }
            : new CncDriverCapabilities
            {
                SupportsAcknowledgements = true,
                SupportsAlarmReset = true,
                SupportsCombinedXyMove = false,
                SupportsLivePositionReporting = false,
                SupportsPause = true,
                SupportsWorkCoordinateSystem = true,
                SupportsZHoming = false,
                VisualizationModelType = "Gantry3Axis"
            };
    }

    public CncDriverType ToCncDriverType(DriverType driverType)
    {
        return driverType == DriverType.Simulated ? CncDriverType.Simulated : CncDriverType.ArduinoSerial;
    }

    public DriverType FromCncDriverType(CncDriverType driverType)
    {
        return driverType == CncDriverType.Simulated ? DriverType.Simulated : DriverType.ArduinoSerial;
    }

    private static CapabilitiesSection ToContractCapabilities(CncDriverCapabilities capabilities, DriverType driverType)
    {
        return new CapabilitiesSection
        {
            Motion = new MotionCapabilities
            {
                Homing = true,
                ZHoming = capabilities.SupportsZHoming,
                CombinedXYHoming = true,
                RelativeMoves = true,
                AbsoluteMoves = true,
                Pause = capabilities.SupportsPause,
                Resume = capabilities.SupportsPause,
                Stop = true,
                Park = true,
                CenterMove = true,
                WorkOffset = capabilities.SupportsWorkCoordinateSystem,
                JogStep = true,
                JogContinuous = false
            },
            Execution = new ExecutionCapabilities
            {
                RealExecution = driverType != DriverType.Simulated,
                Simulation = driverType == DriverType.Simulated,
                PreviewPlayback = true,
                DryRun = true,
                FileRun = true,
                Frame = true,
                BoundingBoxPreview = true,
                LiveReportedPosition = capabilities.SupportsLivePositionReporting,
                EstimatedPositionOnly = !capabilities.SupportsLivePositionReporting,
                ToolpathPreview = true,
                ProgressTracking = true
            },
            Protocol = new ProtocolCapabilities
            {
                Handshake = true,
                Acknowledgements = capabilities.SupportsAcknowledgements,
                AlarmReporting = true,
                AlarmReset = capabilities.SupportsAlarmReset,
                StatusQuery = true,
                PositionQuery = capabilities.SupportsLivePositionReporting,
                MotorEnable = true,
                MotorDisable = true,
                FeedHold = capabilities.SupportsPause,
                SoftReset = true
            },
            Visualization = new VisualizationCapabilities
            {
                MachineVisualization = true,
                TopView2D = true,
                Perspective3D = true,
                KinematicsAnimation = true,
                RealTimePositionDisplay = true
            },
            FileHandling = new FileHandlingCapabilities
            {
                LocalFileRun = true,
                StreamingExecution = true,
                GcodeValidation = true,
                MultipleFileFormats = true
            }
        };
    }
}
