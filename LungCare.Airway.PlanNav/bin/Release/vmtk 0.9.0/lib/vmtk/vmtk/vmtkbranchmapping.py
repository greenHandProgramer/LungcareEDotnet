#!/usr/bin/env python

## Program:   VMTK
## Module:    $RCSfile: vmtkbranchmapping.py,v $
## Language:  Python
## Date:      $Date: 2005/09/14 09:48:31 $
## Version:   $Revision: 1.11 $

##   Copyright (c) Luca Antiga, David Steinman. All rights reserved.
##   See LICENCE file for details.

##      This software is distributed WITHOUT ANY WARRANTY; without even 
##      the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR 
##      PURPOSE.  See the above copyright notices for more information.


import vtk
import vtkvmtk
import sys

import pypes

vmtkbranchmapping = 'vmtkBranchMapping'

class vmtkBranchMapping(pypes.pypeScript):

    def __init__(self):

        pypes.pypeScript.__init__(self)
        
        self.Surface = None
        self.Centerlines = None
        self.ReferenceSystems = None

        self.AbscissasArrayName = ''
        self.NormalsArrayName = ''
        self.GroupIdsArrayName = ''
        self.CenterlineIdsArrayName = ''
        self.TractIdsArrayName = ''
        self.ReferenceSystemsNormalArrayName = ''
        self.RadiusArrayName = ''
        self.BlankingArrayName = ''

        self.AngularMetricArrayName = ''
        self.AbscissaMetricArrayName = ''
        
        self.HarmonicMappingArrayName = 'HarmonicMapping'
        self.BoundaryMetricArrayName = 'BoundaryMetric'
        self.StretchedMappingArrayName = 'StretchedMapping'

        self.SetScriptName('vmtkbranchmapping')
        self.SetScriptDoc('')
        self.SetInputMembers([
            ['Surface','i','vtkPolyData',1,'','','vmtksurfacereader'],
            ['Centerlines','centerlines','vtkPolyData',1,'','','vmtksurfacereader'],
            ['ReferenceSystems','referencesystems','vtkPolyData',1,'','','vmtksurfacereader'],
      	    ['AbscissasArrayName','abscissasarray','str',1],
      	    ['NormalsArrayName','normalsarray','str',1],
      	    ['GroupIdsArrayName','groupidsarray','str',1],
      	    ['CenterlineIdsArrayName','centerlineidsarray','str',1],
      	    ['TractIdsArrayName','tractidsarray','str',1],
      	    ['ReferenceSystemsNormalArrayName','referencesystemsnormalarray','str',1],
      	    ['RadiusArrayName','radiusarray','str',1],
      	    ['BlankingArrayName','blankingarray','str',1],
      	    ['AngularMetricArrayName','angularmetricarray','str',1],
      	    ['HarmonicMappingArrayName','harmonicmappingarray','str',1],
      	    ['AbscissaMetricArrayName','abscissametricarray','str',1],
      	    ['BoundaryMetricArrayName','boundarymetricarray','str',1],
      	    ['StretchedMappingArrayName','stretchedmappingarray','str',1]
            ])
        self.SetOutputMembers([
            ['Surface','o','vtkPolyData',1,'','','vmtksurfacewriter'],
            ['HarmonicMappingArrayName','harmonicmappingarray','str',1],
            ['BoundaryMetricArrayName','boundarymetricarray','str',1],
            ['StretchedMappingArrayName','stretchedmappingarray','str',1]
            ])

    def Execute(self):

        if self.Surface == None:
            self.PrintError('Error: No input surface.')

        if self.Centerlines == None:
            self.PrintError('Error: No input centerlines.')

        if self.ReferenceSystems == None:
            self.PrintError('Error: No input reference systems.')

        self.PrintLog('Computing boundary metric')
        boundaryMetricFilter = vtkvmtk.vtkvmtkPolyDataReferenceSystemBoundaryMetricFilter()
        boundaryMetricFilter.SetInput(self.Surface)
        boundaryMetricFilter.SetBoundaryMetricArrayName(self.BoundaryMetricArrayName)
        boundaryMetricFilter.SetGroupIdsArrayName(self.GroupIdsArrayName)
        boundaryMetricFilter.SetCenterlines(self.Centerlines)
        boundaryMetricFilter.SetCenterlineAbscissasArrayName(self.AbscissasArrayName)
        boundaryMetricFilter.SetCenterlineRadiusArrayName(self.RadiusArrayName)
        boundaryMetricFilter.SetCenterlineGroupIdsArrayName(self.GroupIdsArrayName)
        boundaryMetricFilter.SetCenterlineTractIdsArrayName(self.TractIdsArrayName)
        boundaryMetricFilter.SetCenterlineIdsArrayName(self.CenterlineIdsArrayName)
        boundaryMetricFilter.SetReferenceSystems(self.ReferenceSystems)
        boundaryMetricFilter.SetReferenceSystemGroupIdsArrayName(self.GroupIdsArrayName)
        boundaryMetricFilter.Update()

        self.PrintLog('Computing harmonic mapping')
        harmonicMappingFilter = vtkvmtk.vtkvmtkPolyDataMultipleCylinderHarmonicMappingFilter()
        harmonicMappingFilter.SetInput(boundaryMetricFilter.GetOutput())
        harmonicMappingFilter.SetHarmonicMappingArrayName(self.HarmonicMappingArrayName)
        harmonicMappingFilter.SetGroupIdsArrayName(self.GroupIdsArrayName)
        harmonicMappingFilter.Update()

        self.PrintLog('Stretching harmonic mapping')
        stretchFilter = vtkvmtk.vtkvmtkPolyDataStretchMappingFilter()
        stretchFilter.SetInput(harmonicMappingFilter.GetOutput())
        stretchFilter.SetStretchedMappingArrayName(self.StretchedMappingArrayName)
        stretchFilter.SetHarmonicMappingArrayName(self.HarmonicMappingArrayName)
        stretchFilter.SetGroupIdsArrayName(self.GroupIdsArrayName)
        stretchFilter.SetMetricArrayName(self.AbscissaMetricArrayName)
        stretchFilter.SetBoundaryMetricArrayName(self.BoundaryMetricArrayName)
        stretchFilter.UseBoundaryMetricOn()
        stretchFilter.Update()
        
        self.Surface = stretchFilter.GetOutput()

        if self.Surface.GetSource():
            self.Surface.GetSource().UnRegisterAllOutputs()


if __name__=='__main__':

    main = pypes.pypeMain()
    main.Arguments = sys.argv
    main.Execute()
