#!/usr/bin/env python

## Program:   VMTK
## Module:    $RCSfile: vmtkimagereader.py,v $
## Language:  Python
## Date:      $Date: 2006/05/22 08:33:12 $
## Version:   $Revision: 1.16 $

##   Copyright (c) Luca Antiga, David Steinman. All rights reserved.
##   See LICENCE file for details.

##      This software is distributed WITHOUT ANY WARRANTY; without even 
##      the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR 
##      PURPOSE.  See the above copyright notices for more information.

import vtk
import vtkvmtk
import sys

import pypes

vmtkimagereader = 'vmtkImageReader'

class vmtkImageReader(pypes.pypeScript):

    def __init__(self):

        pypes.pypeScript.__init__(self)
        
        self.Format = ''
        self.GuessFormat = 1
        self.UseITKIO = 1
        self.InputFileName = ''
        self.InputFilePrefix = ''
        self.InputFilePattern = ''
        self.InputDirectoryName = ''
        self.Image = 0
        self.Output = 0

        self.DataExtent = [-1, -1, -1, -1, -1, -1]
        self.DataSpacing = [1.0, 1.0, 1.0]
        self.DataOrigin = [0.0, 0.0, 0.0]
        self.DataByteOrder = 'littleendian'
        self.DataScalarType = 'float'
        self.HeaderSize = 0
        self.FileDimensionality = 3
        self.Flip = [0, 0, 0]
        self.AutoOrientDICOMImage = 1
        self.RasToIjkMatrixCoefficients = [1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1]
        self.XyzToRasMatrixCoefficients = [1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1]

        self.SetScriptName('vmtkimagereader')
        self.SetScriptDoc('read an image and stores it in a vtkImageData object')
        self.SetInputMembers([
            ['Format','f','str',1,'["vtkxml","vtk","dicom","raw","meta","tiff","png"]','file format'],
            ['GuessFormat','guessformat','bool',1,'','guess file format from extension'],
            ['UseITKIO','useitk','bool',1,'','use ITKIO mechanism'],
            ['Image','i','vtkImageData',1,'','the input image'],
            ['InputFileName','ifile','str',1,'','input file name'],
            ['InputFilePrefix','prefix','str',1,'','input file prefix (e.g. foo_)'],
            ['InputFilePattern','pattern','str',1,'','input file pattern (e.g. %s%04d.png)'],
            ['InputDirectoryName','d','str',1,'','input directory name - dicom only'],
            ['DataExtent','extent','int',6,'','3D extent of the image - raw and png'],
            ['HeaderSize','headersize','int',1,'(0,)','size of the image header - raw only'],
            ['DataSpacing','spacing','float',3,'','spacing of the image - raw, tiff, png'],
            ['DataOrigin','origin','float',3,'','origin of the image - raw, tiff, png'],
            ['DataByteOrder','byteorder','str',1,'["littleendian","bigendian"]','byte ordering - raw only'],
            ['DataScalarType','scalartype','str',1,'["float","double","int","short","ushort","uchar"]','scalar type - raw only'],
            ['FileDimensionality','filedimensionality','int',1,'(2,3)','dimensionality of the file to read - raw only'],
            ['Flip','flip','bool',3,'','toggle flipping of the corresponding axis'],
            ['AutoOrientDICOMImage','autoorientdicom','bool',1,'','flip a dicom stack in order to have a left-to-right, posterio-to-anterior, inferior-to-superior image; this is based on the \"image orientation (patient)\" field in the dicom header']
            ])
        self.SetOutputMembers([
            ['Image','o','vtkImageData',1,'','the output image','vmtkimagewriter'],
            ['RasToIjkMatrixCoefficients','rastoijkmatrix','float',16],
            ['XyzToRasMatrixCoefficients','xyztorasmatrix','float',16]
            ])

    def ReadVTKXMLImageFile(self):
        if (self.InputFileName == ''):
            self.PrintError('Error: no InputFileName.')
        self.PrintLog('Reading VTK XML image file.')
        reader = vtk.vtkXMLImageDataReader()
        reader.SetFileName(self.InputFileName)
        reader.Update()
        self.Image = reader.GetOutput()

    def ReadVTKImageFile(self):
        if (self.InputFileName == ''):
            self.PrintError('Error: no InputFileName.')
        self.PrintLog('Reading VTK image file.')
        reader = vtk.vtkStructuredPointsReader()
        reader.SetFileName(self.InputFileName)
        reader.Update()
        self.Image = reader.GetOutput()

    def ReadRawImageFile(self):
        if (self.InputFileName == '') & (self.InputFilePrefix == ''):
            self.PrintError('Error: no InputFileName or InputFilePrefix.')
        self.PrintLog('Reading RAW image file.')
        reader = vtk.vtkImageReader()
        if self.InputFileName != '':
            reader.SetFileName(self.InputFileName)
        else:
            reader.SetFilePrefix(self.InputFilePrefix)
            if self.InputFilePattern != '':
                reader.SetFilePattern(self.InputFilePattern)
            else:
                reader.SetFilePattern("%s%04d.png")
        reader.SetFileDimensionality(self.FileDimensionality)
        if self.DataByteOrder == 'littleendian':
            reader.SetDataByteOrderToLittleEndian()
        elif self.DataByteOrder == 'bigendian':
            reader.SetDataByteOrderToBigEndian()
        reader.SetDataExtent(self.DataExtent)
        reader.SetDataSpacing(self.DataSpacing)
        reader.SetDataOrigin(self.DataOrigin)
      	reader.SetHeaderSize(self.HeaderSize)
      	if self.DataScalarType == 'float':
      	    reader.SetDataScalarTypeToFloat()
       	elif self.DataScalarType == 'double':
      	    reader.SetDataScalarTypeToDouble()
       	elif self.DataScalarType == 'int':
      	    reader.SetDataScalarTypeToInt()
       	elif self.DataScalarType == 'short':
      	    reader.SetDataScalarTypeToShort()
       	elif self.DataScalarType == 'ushort':
      	    reader.SetDataScalarTypeToUnsignedShort()
      	elif self.DataScalarType == 'uchar':
      	    reader.SetDataScalarTypeToUnsignedChar()
        reader.Update()
        self.Image = reader.GetOutput()

    def ReadMetaImageFile(self):
        if (self.InputFileName == ''):
            self.PrintError('Error: no InputFileName.')
        self.PrintLog('Reading meta image file.')
        reader = vtk.vtkMetaImageReader()
        reader.SetFileName(self.InputFileName)
        reader.Update()
        self.Image = reader.GetOutput()

    def ReadTIFFImageFile(self):
        if (self.InputFileName == '') & (self.InputFilePrefix == ''):
            self.PrintError('Error: no InputFileName or InputFilePrefix.')
        self.PrintLog('Reading TIFF image file.')
        reader = vtk.vtkTIFFReader()
        if self.InputFileName != '':
            reader.SetFileName(self.InputFileName)
        else:
            reader.SetFilePrefix(self.InputFilePrefix)
            if self.InputFilePattern != '':
                reader.SetFilePattern(self.InputFilePattern)
            else:
                reader.SetFilePattern("%s%04d.png")
            reader.SetDataExtent(self.DataExtent)
            reader.SetDataSpacing(self.DataSpacing)
            reader.SetDataOrigin(self.DataOrigin)
        reader.Update()
        self.Image = reader.GetOutput()

    def ReadPNGImageFile(self):
        if (self.InputFileName == '') & (self.InputFilePrefix == ''):
            self.PrintError('Error: no InputFileName or InputFilePrefix.')
        self.PrintLog('Reading PNG image file.')
        reader = vtk.vtkPNGReader()
        if self.InputFileName != '':
            reader.SetFileName(self.InputFileName)
        else:
            reader.SetFilePrefix(self.InputFilePrefix)
            if self.InputFilePattern != '':
                reader.SetFilePattern(self.InputFilePattern)
            else:
                reader.SetFilePattern("%s%04d.png")
            reader.SetDataExtent(self.DataExtent)
            reader.SetDataSpacing(self.DataSpacing)
            reader.SetDataOrigin(self.DataOrigin)
        reader.Update()
        self.Image = reader.GetOutput()

    def ReadDICOMFile(self):
        if (self.InputFileName == ''):
            self.PrintError('Error: no InputFileName.')
        self.PrintLog('Reading DICOM file.')
        reader = vtkvmtk.vtkvmtkDICOMImageReader()
        reader.SetFileName(self.InputFileName)
        reader.SetAutoOrientImage(self.AutoOrientDICOMImage)
        reader.Update()
        self.Image = reader.GetOutput()

    def ReadDICOMDirectory(self):
        if (self.InputDirectoryName == ''):
            self.PrintError('Error: no InputDirectoryName.')
        self.PrintLog('Reading DICOM directory.')
        reader = vtkvmtk.vtkvmtkDICOMImageReader()
        reader.SetDirectoryName(self.InputDirectoryName)
        reader.SetAutoOrientImage(self.AutoOrientDICOMImage)
        reader.Update()
        self.Image = reader.GetOutput()

    def ReadITKIO(self):
        if self.InputFileName == '':
            self.PrintError('Error: no InputFileName.')
        reader = vtkvmtk.vtkvmtkITKArchetypeImageSeriesScalarReader()
        reader.SetArchetype(self.InputFileName)
        reader.SetOutputScalarTypeToNative()
        reader.SetDesiredCoordinateOrientationToNative()
        reader.SetSingleFile(0)
        reader.Update()
        self.Image = vtk.vtkImageData()
        self.Image.DeepCopy(reader.GetOutput())
        matrix = reader.GetRasToIjkMatrix()
        self.RasToIjkMatrixCoefficients = [
            matrix.GetElement(0,0), matrix.GetElement(0,1), matrix.GetElement(0,2), matrix.GetElement(0,3),
            matrix.GetElement(1,0), matrix.GetElement(1,1), matrix.GetElement(1,2), matrix.GetElement(1,3),
            matrix.GetElement(2,0), matrix.GetElement(2,1), matrix.GetElement(2,2), matrix.GetElement(2,3),
            matrix.GetElement(3,0), matrix.GetElement(3,1), matrix.GetElement(3,2), matrix.GetElement(3,3)]

        matrix.Invert()
        origin = [matrix.GetElement(0,3), matrix.GetElement(1,3), matrix.GetElement(2,3)] 
        translationToOrigin = [-origin[0], -origin[1], -origin[2]] 

        for i in range(3):
            direction = [matrix.GetElement(0,i), matrix.GetElement(1,i), matrix.GetElement(2,i)]
            vtk.vtkMath.Normalize(direction)
            matrix.SetElement(0,i,direction[0])
            matrix.SetElement(1,i,direction[1])
            matrix.SetElement(2,i,direction[2])
        matrix.SetElement(0,3,0.0)
        matrix.SetElement(1,3,0.0)
        matrix.SetElement(2,3,0.0)
 
        transform = vtk.vtkTransform()
        transform.PostMultiply()
        transform.Translate(translationToOrigin)
        transform.Concatenate(matrix)
        transform.Translate(origin)

        matrix = transform.GetMatrix()
        self.XyzToRasMatrixCoefficients = [
            matrix.GetElement(0,0), matrix.GetElement(0,1), matrix.GetElement(0,2), matrix.GetElement(0,3),
            matrix.GetElement(1,0), matrix.GetElement(1,1), matrix.GetElement(1,2), matrix.GetElement(1,3),
            matrix.GetElement(2,0), matrix.GetElement(2,1), matrix.GetElement(2,2), matrix.GetElement(2,3),
            matrix.GetElement(3,0), matrix.GetElement(3,1), matrix.GetElement(3,2), matrix.GetElement(3,3)]


    def Execute(self):

        extensionFormats = {'vti':'vtkxml', 
                            'vtkxml':'vtkxml', 
                            'vtk':'vtk',
                            'dcm':'dicom',
                            'raw':'raw',
                            'mhd':'meta',
                            'mha':'meta',
                            'tif':'tiff',
                            'png':'png'}

        if self.InputFileName == 'BROWSER':
            import tkFileDialog
            initialDir = '.'
            self.InputFileName = tkFileDialog.askopenfilename(title="Input image",initialdir=initialDir)
            if not self.InputFileName:
                self.PrintError('Error: no InputFileName.')

        if self.InputDirectoryName == 'BROWSER':
            import tkFileDialog
            initialDir = '.'
            self.InputDirectoryName = tkFileDialog.askdirectory(title="Input directory",initialdir=initialDir)
            if not self.InputDirectoryName:
                self.PrintError('Error: no InputDirectoryName.')

        if self.GuessFormat and self.InputFileName and not self.Format:
            import os.path
            extension = os.path.splitext(self.InputFileName)[1]
            if extension:
                extension = extension[1:]
                if extension in extensionFormats.keys():
                    self.Format = extensionFormats[extension]

        if self.UseITKIO and self.InputFileName and self.Format not in ['vtkxml','raw'] and not self.InputDirectoryName:
            self.ReadITKIO()    
        else:
            if self.Format == 'vtkxml':
                self.ReadVTKXMLImageFile()
            elif self.Format == 'vtk':
                self.ReadVTKImageFile()
            elif self.Format == 'dicom':
                if self.InputDirectoryName != '':
                    self.ReadDICOMDirectory()
                else:
                    self.ReadDICOMFile()
            elif self.Format == 'raw':
                self.ReadRawImageFile()
            elif self.Format == 'meta':
                self.ReadMetaImageFile()
            elif self.Format == 'png':
                self.ReadPNGImageFile()
            elif self.Format == 'tiff':
                self.ReadTIFFImageFile()
            else:
                self.PrintError('Error: unsupported format '+ self.Format + '.')

        if (self.Flip[0] == 1) | (self.Flip[1] == 1) | (self.Flip[2] == 1):
            temp0 = self.Image
            if self.Flip[0] == 1:
                flipFilter = vtk.vtkImageFlip()
                flipFilter.SetInput(self.Image)
                flipFilter.SetFilteredAxis(0)
                flipFilter.Update()
                temp0 = flipFilter.GetOutput()
            temp1 = temp0
            if self.Flip[1] == 1:
                flipFilter = vtk.vtkImageFlip()
                flipFilter.SetInput(temp0)
                flipFilter.SetFilteredAxis(1)
                flipFilter.Update()
                temp1 = flipFilter.GetOutput()
            temp2 = temp1
            if self.Flip[2] == 1:
                flipFilter = vtk.vtkImageFlip()
                flipFilter.SetInput(temp1)
                flipFilter.SetFilteredAxis(2)
                flipFilter.Update()
                temp2 = flipFilter.GetOutput()
            self.Image = temp2

        print 'Spacing ', self.Image.GetSpacing()
        print 'Origin ', self.Image.GetOrigin()
        print 'Dimensions ', self.Image.GetDimensions()

        if self.Image.GetSource():
            self.Image.GetSource().UnRegisterAllOutputs()

        self.Output = self.Image

if __name__=='__main__':
    main = pypes.pypeMain()
    main.Arguments = sys.argv
    main.Execute()

