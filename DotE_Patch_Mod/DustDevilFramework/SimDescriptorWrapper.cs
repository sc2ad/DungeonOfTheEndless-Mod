using Amplitude.Unity.Simulation;
using Amplitude.Unity.Simulation.SimulationModifierDescriptors;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace DustDevilFramework
{
    public class SimDescriptorWrapper
    {
        public List<SimulationModifierDescriptor> Modifiers { get; set; } = new List<SimulationModifierDescriptor>();
        public List<SimulationPropertyDescriptor> Properties { get; set; } = new List<SimulationPropertyDescriptor>();

        // Should only input SimulationProperties as a parameter
        public void Add(Amplitude.StaticString name, float value)
        {
            SingleSimulationModifierDescriptor modif = new SingleSimulationModifierDescriptor(name, SimulationModifierDescriptor.ModifierOperation.Addition, value);
            Properties.Add(new SimulationPropertyDescriptor(name));
            Modifiers.Add(modif);
        }
        public void Add(Amplitude.StaticString name, float startingValue, SimulationModifierDescriptor modif = null)
        {
            SimulationPropertyDescriptor desc = new SimulationPropertyDescriptor(name);
            new DynData<SimulationPropertyDescriptor>(desc).Set<float>("BaseValue", startingValue);
            Properties.Add(desc);
            if (modif != null)
            {
                Modifiers.Add(modif);
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

            descriptor.SetProperties(Properties.ToArray());
            descriptor.SetModifiers(Modifiers.ToArray());

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
