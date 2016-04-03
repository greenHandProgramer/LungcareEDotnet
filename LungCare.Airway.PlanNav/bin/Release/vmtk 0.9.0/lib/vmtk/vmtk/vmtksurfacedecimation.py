#!/usr/bin/env python

## Program:   VMTK
## Module:    $RCSfile: vmtksurfacedecimation.py,v $
## Language:  Python
## Date:      $Date: 2006/02/23 09:27:52 $
## Version:   $Revision: 1.7 $

##   Copyright (c) Luca Antiga, David Steinman. All rights reserved.
##   See LICENCE file for details.

##      This software is distributed WITHOUT ANY WARRANTY; without even 
##      the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR 
##      PURPOSE.  See the above copyright notices for more information.


import vtk
import sys

import pypes

vmtksurfacedecimation = 'vmtkSurfaceDecimation'

class vmtkSurfaceDecimation(pypes.pypeScript):

    def __init__(self):

        pypes.pypeScript.__init__(self)
        
        self.Surface = None

        self.TargetReduction = 0.5
        self.BoundaryVertexDeletion = 0

        self.SetScriptName('vmtksurfacedecimation')
        self.SetScriptDoc('reduce the number of triangles in a surface')
        self.SetInputMembers([
            ['Surface','i','vtkPolyData',1,'','the input surface','vmtksurfacereader'],
            ['TargetReduction','reduction','float',1,'(0.0,1.0)','desired number of triangles relative to input number of triangles'],
            ['BoundaryVertexDeletion','boundarydeletion','bool',1,'','toggle allow boundary point deletion']
            ])
        self.SetOutputMembers([
            ['Surface','o','vtkPolyData',1,'','the output surface','vmtksurfacewriter']
            ])

    def Execute(self):

        if self.Surface == None:
            self.PrintError('Error: No input surface.')

        triangleFilter = vtk.vtkTriangleFilter()
        triangleFilter.SetInput(self.Surface)
        triangleFilter.Update()

        decimationFilter = vtk.vtkDecimatePro()
        decimationFilter.SetInput(triangleFilter.GetOutput())
        decimationFilter.SetTargetReduction(self.TargetReduction)
        decimationFilter.SetBoundaryVertexDeletion(self.BoundaryVertexDeletion)
        decimationFilter.PreserveTopologyOn()
        decimationFilter.Update()

        cleaner = vtk.vtkCleanPolyData()
        cleaner.SetInput(decimationFilter.GetOutput())
        cleaner.Update()

        triangleFilter = vtk.vtkTriangleFilter()
        triangleFilter.SetInput(cleaner.GetOutput())
        triangleFilter.Update()

        self.Surface = triangleFilter.GetOutput()

        if self.Surface.GetSource():
            self.Surface.GetSource().UnRegisterAllOutputs()


if __name__=='__main__':

    main = pypes.pypeMain()
    main.Arguments = sys.argv
    main.Execute()
