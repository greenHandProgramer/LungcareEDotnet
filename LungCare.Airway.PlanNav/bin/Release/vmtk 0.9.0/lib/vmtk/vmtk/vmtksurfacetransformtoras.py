#!/usr/bin/env python

## Program:   VMTK
## Module:    $RCSfile: vmtksurfacetransformtoras.py,v $
## Language:  Python
## Date:      $Date: Sun Feb 21 17:02:37 CET 2010$
## Version:   $Revision: 1.0 $

##   Copyright (c) Luca Antiga, David Steinman. All rights reserved.
##   See LICENCE file for details.

##      This software is distributed WITHOUT ANY WARRANTY; without even 
##      the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR 
##      PURPOSE.  See the above copyright notices for more information.

import vtk
import sys

import pypes

vmtksurfacetransformtoras = 'vmtkSurfaceTransformToRAS'

class vmtkSurfaceTransformToRAS(pypes.pypeScript):

    def __init__(self):

        pypes.pypeScript.__init__(self)

        self.Surface = None
        self.XyzToRasMatrixCoefficients = None

        self.SetScriptName('vmtksurfacetransformtoras')
        self.SetScriptDoc('transform a surface generated in XYZ image space into RAS space')
        self.SetInputMembers([
            ['Surface','i','vtkPolyData',1,'','the input surface','vmtksurfacereader'],
            ['XyzToRasMatrixCoefficients','matrix','float',16,'','coefficients of XYZToRAS transform matrix']
            ])
        self.SetOutputMembers([
            ['Surface','o','vtkPolyData',1,'','the output surface','vmtksurfacewriter']
            ])

    def Execute(self):

        if self.Surface == None:
            self.PrintError('Error: no Surface.')

        if self.XyzToRasMatrixCoefficients == None:
            self.PrintError('Error: no XyzToRasMatrixCoefficients.')

        matrix = vtk.vtkMatrix4x4()
        matrix.DeepCopy(self.XyzToRasMatrixCoefficients)

        transform = vtk.vtkTransform()
        transform.SetMatrix(matrix)

        transformFilter = vtk.vtkTransformPolyDataFilter()
        transformFilter.SetInput(self.Surface)
        transformFilter.SetTransform(transform)
        transformFilter.Update()

        self.Surface = transformFilter.GetOutput()

        if self.Surface.GetSource():
            self.Surface.GetSource().UnRegisterAllOutputs()


if __name__=='__main__':
    main = pypes.pypeMain()
    main.Arguments = sys.argv
    main.Execute()
