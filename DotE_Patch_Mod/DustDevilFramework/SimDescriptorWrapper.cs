using Amplitude.Unity.Simulation;
using Amplitude.Unity.Simulation.SimulationModifierDescriptors;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DustDevilFramework
{
    public class SimDescriptorWrapper
    {
        public List<SimulationModifierDescriptor> Modifiers { get { return simulationModifierDescriptors; } set { simulationModifierDescriptors = value; } }
        public List<SimulationPropertyDescriptor> Properties { get { return simulationPropertyDescriptors; } set { simulationPropertyDescriptors = value; } }
        List<SimulationModifierDescriptor> simulationModifierDescriptors = new List<SimulationModifierDescriptor>();
        List<SimulationPropertyDescriptor> simulationPropertyDescriptors = new List<SimulationPropertyDescriptor>();
        // Should only input SimulationProperties as a parameter
        public void Add(Amplitude.StaticString name, float value)
        {
            SingleSimulationModifierDescriptor modif = new SingleSimulationModifierDescriptor(name, SimulationModifierDescriptor.ModifierOperation.Addition, value);
            simulationPropertyDescriptors.Add(new SimulationPropertyDescriptor(name));
            simulationModifierDescriptors.Add(modif);
        }
        public void Add(Amplitude.StaticString name, float startingValue, SimulationModifierDescriptor modif = null)
        {
            SimulationPropertyDescriptor desc = new SimulationPropertyDescriptor(name);
            new DynData<SimulationPropertyDescriptor>(desc).Set<float>("BaseValue", startingValue);
            simulationPropertyDescriptors.Add(desc);
            if (modif != null)
            {
                simulationModifierDescriptors.Add(modif);
            }
        }
        public void Add(string targetName, float value, SimulationModifierDescriptor.ModifierOperation operation = SimulationModifierDescriptor.ModifierOperation.Addition)
        {
            Modifiers.Add(new SingleSimulationModifierDescriptor(targetName, operation, value));
        }
        public SimulationDescriptor GetDescriptor(string name)
        {
            SimulationDescriptor descriptor = new SimulationDescriptor();
            descriptor.SetName(name);

            descriptor.SetProperties(simulationPropertyDescriptors.ToArray());
            descriptor.SetModifiers(simulationModifierDescriptors.ToArray());

            return descriptor;
        }
        public SimulationDescriptor GetDescriptor(string name, Amplitude.StaticString type)
        {
            var temp = GetDescriptor(name);
            if (type != null)
            {
                temp.SetType(type);
            }
            return temp;
        }
    }
}
