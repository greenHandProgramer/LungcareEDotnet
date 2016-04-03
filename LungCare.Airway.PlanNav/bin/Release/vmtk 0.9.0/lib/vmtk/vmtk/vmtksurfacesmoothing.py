#!/usr/bin/env python

## Program:   VMTK
## Module:    $RCSfile: vmtksurfacesmoothing.py,v $
## Language:  Python
## Date:      $Date: 2006/07/17 09:53:14 $
## Version:   $Revision: 1.8 $

##   Copyright (c) Luca Antiga, David Steinman. All rights reserved.
##   See LICENCE file for details.

##      This software is distributed WITHOUT ANY WARRANTY; without even 
##      the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR 
##      PURPOSE.  See the above copyright notices for more information.


import vtk
import sys

import pypes
import vmtkscripts

vmtksurfacesmoothing = 'vmtkSurfaceSmoothing'

class vmtkSurfaceSmoothing(pypes.pypeScript):

    def __init__(self):

        pypes.pypeScript.__init__(self)
        
        self.Surface = None

        self.NumberOfIterations = 1
        self.PassBand = 1.0
        self.RelaxationFactor = 0.01
        self.BoundarySmoothing = 1
       
        self.Method = 'taubin' 

        self.SetScriptName('vmtksurfacesmoothing')
        self.SetScriptDoc('smooth a surface using Taubin\'s algorithm')
        self.SetInputMembers([
            ['Surface','i','vtkPolyData',1,'','the input surface','vmtksurfacereader'],
            ['NumberOfIterations','iterations','int',1,'(0,)','number of iterations (e.g. 25)'],
            ['Method','method','str',1,'["taubin","laplace"]','smoothing method'],
            ['PassBand','passband','float',1,'','pass band (e.g. 0.1) - taubin only'],
            ['RelaxationFactor','relaxation','float',1,'(0.0,)','relaxation factor (e.g. 0.01) - laplace only'],
            ['BoundarySmoothing','boundarysmoothing','bool',1,'','toggle allow change of position of boundary points']
            ])
        self.SetOutputMembers([
            ['Surface','o','vtkPolyData',1,'','the output surface','vmtksurfacewriter']
            ])

    def Execute(self):

        if self.Surface == None:
            self.PrintError('Error: No input surface.')

        smoothingFilter = None

        if self.Method is 'taubin':
            smoothingFilter = vtk.vtkWindowedSincPolyDataFilter()
            smoothingFilter.SetInput(self.Surface)
            smoothingFilter.SetNumberOfIterations(self.NumberOfIterations)
            smoothingFilter.SetPassBand(self.PassBand)
            smoothingFilter.SetBoundarySmoothing(self.BoundarySmoothing)
##            smoothingFilter.NormalizeCoordinatesOn()
            smoothingFilter.Update()
        elif self.Method is 'laplace':
            smoothingFilter = vtk.vtkSmoothPolyDataFilter()
            smoothingFilter.SetInput(self.Surface)
            smoothingFilter.SetNumberOfIterations(self.NumberOfIterations)
            smoothingFilter.SetRelaxationFactor(self.RelaxationFactor)
            smoothingFilter.Update()
        else:
            self.PrintError('Error: smoothing method not supported.')

        self.Surface = smoothingFilter.GetOutput()

        normals = vmtkscripts.vmtkSurfaceNormals()
        normals.Surface = self.Surface
        normals.Execute()

        self.Surface = normals.Surface

        if self.Surface.GetSource():
            self.Surface.GetSource().UnRegisterAllOutputs()


if __name__=='__main__':

    main = pypes.pypeMain()
    main.Arguments = sys.argv
    main.Execute()
