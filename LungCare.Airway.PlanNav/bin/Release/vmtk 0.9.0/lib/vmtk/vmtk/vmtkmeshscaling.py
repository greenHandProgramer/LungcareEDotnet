#!/usr/bin/env python

## Program:   VMTK
## Module:    $RCSfile: vmtkmeshscaling.py,v $
## Language:  Python
## Date:      $Date: 2005/09/14 09:49:59 $
## Version:   $Revision: 1.6 $

##   Copyright (c) Luca Antiga, David Steinman. All rights reserved.
##   See LICENCE file for details.

##      This software is distributed WITHOUT ANY WARRANTY; without even 
##      the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR 
##      PURPOSE.  See the above copyright notices for more information.

import vtk
import sys

import pypes

vmtkmeshscaling = 'vmtkMeshScaling'

class vmtkMeshScaling(pypes.pypeScript):

    def __init__(self):

        pypes.pypeScript.__init__(self)

        self.Mesh = None

        self.ScaleFactor = None

        self.SetScriptName('vmtkmeshscaling')
        self.SetScriptDoc('scale a mesh by an isotropic factor')
        self.SetInputMembers([
            ['Mesh','i','vtkUnstructuredGrid',1,'','the input mesh','vmtkmeshreader'],
            ['ScaleFactor','scale','float',1,'(0.0,)','isotropic scaling factor']
            ])
        self.SetOutputMembers([
            ['Mesh','o','vtkUnstructuredGrid',1,'','the output mesh','vmtkmeshwriter']
            ])

    def Execute(self):

        if (self.Mesh == None):
            self.PrintError('Error: no Mesh.')

        transform = vtk.vtkTransform()
        transform.Scale(self.ScaleFactor,self.ScaleFactor,self.ScaleFactor)

        transformFilter = vtk.vtkTransformFilter()
        transformFilter.SetInput(self.Mesh)
        transformFilter.SetTransform(transform)
        transformFilter.Update()

        self.Mesh = transformFilter.GetOutput()

        if self.Mesh.GetSource():
            self.Mesh.GetSource().UnRegisterAllOutputs()


if __name__=='__main__':
    main = pypes.pypeMain()
    main.Arguments = sys.argv
    main.Execute()
