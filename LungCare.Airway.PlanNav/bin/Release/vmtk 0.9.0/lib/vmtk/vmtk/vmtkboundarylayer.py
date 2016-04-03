#!/usr/bin/env python

## Program:   VMTK
## Module:    $RCSfile: vmtkboundarylayer.py,v $
## Language:  Python
## Date:      $Date: 2005/09/14 09:49:59 $
## Version:   $Revision: 1.6 $

##   Copyright (c) Luca Antiga, David Steinman. All rights reserved.
##   See LICENCE file for details.

##      This software is distributed WITHOUT ANY WARRANTY; without even 
##      the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR 
##      PURPOSE.  See the above copyright notices for more information.


import vtk
import vtkvmtk
import sys

import pypes

vmtkboundarylayer = 'vmtkBoundaryLayer'

class vmtkBoundaryLayer(pypes.pypeScript):

    def __init__(self):

        pypes.pypeScript.__init__(self)

        self.Mesh = None
        self.InnerSurfaceMesh = None
        
        self.WarpVectorsArrayName = ''
        self.ThicknessArrayName = ''

        self.Thickness = 1.0
        self.ThicknessRatio = 0.1
        self.MaximumThickness = 1E10

        self.NumberOfSubLayers = 1
        self.SubLayerRatio = 1.0

        self.UseWarpVectorMagnitudeAsThickness = 0;
        self.ConstantThickness = 0;
        self.IncludeSurfaceCells = 1
        self.NegateWarpVectors = 0

        self.SetScriptName('vmtkboundarylayer')
        self.SetScriptDoc('create a prismatic boundary layer from a surface mesh and a set of vectors defined on the nodes')
        self.SetInputMembers([
            ['Mesh','i','vtkUnstructuredGrid',1,'','the input mesh','vmtkmeshreader'],
            ['WarpVectorsArrayName','warpvectorsarray','str',1,'','name of the array where warp vectors are stored'],
            ['ThicknessArrayName','thicknessarray','str',1,'','name of the array where scalars defining boundary layer thickness are stored'],
            ['Thickness','thickness','float',1,'','value of constant boundary layer thickness'],
            ['ThicknessRatio','thicknessratio','float',1,'(0.0,)','multiplying factor for boundary layer thickness'],
            ['MaximumThickness','maximumthickness','float',1,'','maximum allowed value for boundary layer thickness'],
            ['NumberOfSubLayers','numberofsublayers','int',1,'(0,)','number of sublayers which the boundary layer has to be made of'],
            ['SubLayerRatio','sublayerratio','float',1,'(0.0,)','ratio between the thickness of two successive boundary layers'],
            ['UseWarpVectorMagnitudeAsThickness','warpvectormagnitudeasthickness','bool',1,'','compute boundary layer thickness as the norm of warp vectors'],
            ['ConstantThickness','constantthickness','bool',1,'','toggle constant boundary layer thickness'],
            ['IncludeSurfaceCells','includesurfacecells','bool',1,'','include surface cells in output mesh'],
            ['NegateWarpVectors','negatewarpvectors','bool',1,'','flip the orientation of warp vectors']
            ])
        self.SetOutputMembers([
            ['Mesh','o','vtkUnstructuredGrid',1,'','the output mesh','vmtkmeshwriter'],
            ['InnerSurfaceMesh','oinner','vtkUnstructuredGrid',1,'','the output inner surface mesh','vmtkmeshwriter']
            ])

    def Execute(self):

        if self.Mesh == None:
            self.PrintError('Error: No input mesh.')

        boundaryLayerGenerator = vtkvmtk.vtkvmtkBoundaryLayerGenerator()
        boundaryLayerGenerator.SetInput(self.Mesh)
        boundaryLayerGenerator.SetWarpVectorsArrayName(self.WarpVectorsArrayName)
        boundaryLayerGenerator.SetLayerThickness(self.Thickness)
        boundaryLayerGenerator.SetLayerThicknessArrayName(self.ThicknessArrayName)
        boundaryLayerGenerator.SetLayerThicknessRatio(self.ThicknessRatio)
        boundaryLayerGenerator.SetMaximumLayerThickness(self.MaximumThickness)
        boundaryLayerGenerator.SetNumberOfSubLayers(self.NumberOfSubLayers)
        boundaryLayerGenerator.SetSubLayerRatio(self.SubLayerRatio)
        boundaryLayerGenerator.SetConstantThickness(self.ConstantThickness)
        boundaryLayerGenerator.SetUseWarpVectorMagnitudeAsThickness(self.UseWarpVectorMagnitudeAsThickness)
        boundaryLayerGenerator.SetIncludeSurfaceCells(self.IncludeSurfaceCells)
        boundaryLayerGenerator.SetNegateWarpVectors(self.NegateWarpVectors)
        boundaryLayerGenerator.Update()
        
        self.Mesh = boundaryLayerGenerator.GetOutput()
        self.InnerSurfaceMesh = boundaryLayerGenerator.GetInnerSurface()

        if self.Mesh.GetSource():
            self.Mesh.GetSource().UnRegisterAllOutputs()


if __name__=='__main__':

    main = pypes.pypeMain()
    main.Arguments = sys.argv
    main.Execute()
