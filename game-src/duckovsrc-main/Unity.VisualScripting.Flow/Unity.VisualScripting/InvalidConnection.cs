using System;
using System.Linq;

namespace Unity.VisualScripting;

public sealed class InvalidConnection : UnitConnection<IUnitOutputPort, IUnitInputPort>, IUnitConnection, IConnection<IUnitOutputPort, IUnitInputPort>, IGraphElementWithDebugData, IGraphElement, IGraphItem, INotifiedCollectionItem, IDisposable, IPrewarmable, IAotStubbable, IIdentifiable, IAnalyticsIdentifiable
{
	public override IUnitOutputPort source => base.sourceUnit.outputs.Single((IUnitOutputPort p) => p.key == base.sourceKey);

	public override IUnitInputPort destination => base.destinationUnit.inputs.Single((IUnitInputPort p) => p.key == base.destinationKey);

	public IUnitOutputPort validSource => base.sourceUnit.validOutputs.Single((IUnitOutputPort p) => p.key == base.sourceKey);

	public IUnitInputPort validDestination => base.destinationUnit.validInputs.Single((IUnitInputPort p) => p.key == base.destinationKey);

	public override bool sourceExists => base.sourceUnit.outputs.Any((IUnitOutputPort p) => p.key == base.sourceKey);

	public override bool destinationExists => base.destinationUnit.inputs.Any((IUnitInputPort p) => p.key == base.destinationKey);

	public bool validSourceExists => base.sourceUnit.validOutputs.Any((IUnitOutputPort p) => p.key == base.sourceKey);

	public bool validDestinationExists => base.destinationUnit.validInputs.Any((IUnitInputPort p) => p.key == base.destinationKey);

	[Obsolete("This parameterless constructor is only made public for serialization. Use another constructor instead.")]
	public InvalidConnection()
	{
	}

	public InvalidConnection(IUnitOutputPort source, IUnitInputPort destination)
		: base(source, destination)
	{
	}

	public override void AfterRemove()
	{
		base.AfterRemove();
		source.unit.RemoveUnconnectedInvalidPorts();
		destination.unit.RemoveUnconnectedInvalidPorts();
	}

	public override bool HandleDependencies()
	{
		if (validSourceExists && validDestinationExists && validSource.CanValidlyConnectTo(validDestination))
		{
			validSource.ValidlyConnectTo(validDestination);
			return false;
		}
		if (!sourceExists)
		{
			base.sourceUnit.invalidOutputs.Add(new InvalidOutput(base.sourceKey));
		}
		if (!destinationExists)
		{
			base.destinationUnit.invalidInputs.Add(new InvalidInput(base.destinationKey));
		}
		return true;
	}

	FlowGraph IUnitConnection.get_graph()
	{
		return base.graph;
	}
}
