using UnityEngine;
using System.Collections;
using MOL;	//necessary

public class Sample : MonoBehaviour {

	/*-------*/
	/*-------*/
	void HowToSetVector_case1() {
		Matrix Vec = new Matrix(3, 1); //vector
		Vec[0, 0] = 1.0;
		Vec[1, 0] = 4.0;
		Vec[2, 0] = 3.0;

		Debug.LogFormat("How to set vector 1:\n{0}", Vec.ToString());
	}

	/*-------*/
	/*-------*/
	void HowToSetVector_case2(){
		double[] ArrVec = new double[4] { 3.0, 1.0, 2.0, 5.0 };
		Matrix Vec = new Matrix(ArrVec);

		Debug.LogFormat("How to set vector 2:\n{0}", Vec.ToString());
	}

	/*-------*/
	/*-------*/
	void HowToSetVector_case3()
	{
		Matrix Vec = new Matrix(3, 1); //vector
		Vec[0] = 1.0;
		Vec[1] = 4.0;
		Vec[2] = 3.0;

		Debug.LogFormat("How to set vector 3:\n{0}", Vec.ToString());
	}

	/*-------*/
	/*-------*/
	void HowToSetVector_AllOne()
	{
		Matrix Vec = new Matrix(3);
		Vec.One();

		Debug.LogFormat("How to set vector One :\n{0}", Vec.ToString());
	}

	/*-------*/
	/*-------*/
	void HowToSetVector_Random() {
		Matrix Vec = new Matrix(5);
		Vec.Random();	//each elements are setted 0.0~1.0 value.

		Debug.LogFormat("How to set vector Random :\n{0}", Vec.ToString());
	}

	/*-------*/
	/*-------*/
	void HowToSetMatrix_case1() {
		Matrix Mat = new Matrix(3, 3);
		Mat[0, 0] = 1.0; Mat[0, 1] = 2.0; Mat[0, 2] = 3.0;
		Mat[1, 0] = 5.0; Mat[1, 1] = 1.0; Mat[1, 2] = 6.0;
		Mat[2, 0] = 7.0; Mat[2, 1] = 4.0; Mat[2, 2] = 5.0;

		Debug.LogFormat("How to set matrix 1 :\n{0}", Mat.ToString());
	}

	/*-------*/
	/*-------*/
	void HowToSetMatrix_case2()
	{
		double[,] ArrMat = new double[4, 4] {  { 1.0, 2.0, 1.0, 5.0 }, 
												{ 2.0, 1.0, 0.0, 2.0 }, 
												{ 1.0, 1.0, 2.0, 9.0 },
												{ 6.0, 3.0, 7.0, 1.0} };
		Matrix Mat = new Matrix(ArrMat);

		Debug.LogFormat("How to set matrix 2 :\n{0}", Mat.ToString());
	}

	/*-------*/
	/*-------*/
	void HowToSetMatrix_case3()
	{
		Matrix Mat = new Matrix(3, 3);
		Mat[0, 0] = 1.0; Mat[0, 1] = 2.0; Mat[0, 2] = 3.0;
		Mat[1, 0] = 5.0; Mat[1, 1] = 1.0; Mat[1, 2] = 6.0;
		Mat[2, 0] = 7.0; Mat[2, 1] = 4.0; Mat[2, 2] = 5.0;

		Debug.LogFormat("How to set matrix 3 :\n{0}", Mat.ToString());
	}

	/*-------*/
	/*-------*/
	void HowToSetMatrix_AllOne()
	{
		Matrix Mat = new Matrix(3, 3);  //row: 3 / column: 3
		Mat.One();

		Debug.LogFormat("How to set matrix All One :\n{0}", Mat.ToString());
	}

	/*-------*/
	/*-------*/
	void HowToSetMatrix_Identity()
	{
		Matrix Mat = new Matrix(5, 5);
		Mat.Identity();

		Debug.LogFormat("How to set Identity matrix :\n{0}", Mat.ToString());
	}

	/*-------*/
	/*-------*/
	void HowToSetMatrix_Random()
	{
		Matrix Mat = new Matrix(4, 3);  //row: 4 / column: 3
		Mat.Random();

		Debug.LogFormat("How to set matrix Random :\n{0}", Mat.ToString());
	}

	/*-------*/
	/*-------*/
	void AdditionOfVectors()
	{
		Matrix VecA = new Matrix(new double[] { 1.0, 2.0, 3.0 });
		Matrix VecB = new Matrix(new double[] { 2.0, 1.0, 5.0 });

		Matrix VecAns = VecA + VecB;

		Debug.LogFormat("VecA + VecB =\n{0}", VecAns.ToString());
	}

	/*-------*/
	/*-------*/
	void SubtractionOfVectors()
	{
		Matrix VecA = new Matrix(new double[] { 1.0, 2.0, 3.0 });
		Matrix VecB = new Matrix(new double[] { 2.0, 1.0, 5.0 });

		Matrix VecAns = VecA - VecB;

		Debug.LogFormat("VecA - VecB =\n{0}", VecAns.ToString());
	}

	/*-------*/
	/*-------*/
	void TranspositionOfVector() {
		Matrix Vec = new Matrix(new double[4] {1.0, 2.0, 3.0, 4.0 });
		Matrix VecT = Vec.Transposition();

		string str = "";
		str += "Vec = \n";
		str += Vec.ToString();
		str += "\n";
		str += "Vec T (Transposed) = \n";
		str += VecT.ToString();
		Debug.LogFormat("{0}", str);
	}

	/*-------*/
	/*-------*/
	void TranspositionOfMatrix()
	{
		double[,] ArrMat = new double[3, 3] {   { 1.0, 4.0, 8.0 },
												{ 3.0, 2.0, 6.0 },
												{ 5.0, 9.0, 3.0 } };
		Matrix Mat = new Matrix(ArrMat);
		Matrix MatT = Mat.Transposition();

		string str = "";
		str += "Mat = \n";
		str += Mat.ToString();
		str += "\n";
		str += "Mat T (Transposed) = \n";
		str += MatT.ToString();
		Debug.LogFormat("{0}", str);
	}

	/*-------*/
	/*-------*/
	void Multiplication_VectorT_Vector() {
		Matrix VecA = new Matrix(new double[] { 1.0, 2.0, 3.0 });
		Matrix VecB = new Matrix(new double[] { 2.0, 3.0, 4.0 });

		Matrix Ans = VecA.Transposition() * VecB;
		Debug.LogFormat("VecA_T * VecB = \n{0}", Ans);
	}

	/*-------*/
	/*-------*/
	void Multiplication_Vector_VectorT()
	{
		Matrix VecA = new Matrix(new double[] { 1.0, 2.0, 3.0 });
		Matrix VecB = new Matrix(new double[] { 2.0, 3.0, 4.0 });

		Matrix Ans = VecA * VecB.Transposition();
		Debug.LogFormat("VecA * VecB_T = \n{0}", Ans);
	}

	/*-------*/
	/*-------*/
	void Multiplication_Matrix_Vector() {
		double[,] ArrMat = new double[3, 3] {   { 1.0, 4.0, 8.0 },
												{ 3.0, 2.0, 6.0 },
												{ 5.0, 9.0, 3.0 } };
		Matrix Mat = new Matrix(ArrMat);
		Matrix Vec = new Matrix(new double[] { 1.0, 2.0, 3.0 });

		Matrix Ans = Mat * Vec;

		Debug.LogFormat("Mat(3, 3) * Vec(3) = \n{0}", Ans);
	}

	/*-------*/
	/*-------*/
	void DeterminantOfMatrix(){
		double[,] ArrMat = new double[3, 3] {   { 1.0, 4.0, 8.0 },
												{ 3.0, 2.0, 6.0 },
												{ 5.0, 9.0, 3.0 } };
		Matrix Mat = new Matrix(ArrMat);
		double Ans = Mat.Determinant();
		
		Debug.LogFormat("Determinant of Matrix: {0}", Ans);
	}

	/*-------*/
	/*-------*/
	void InverseOfMatrix()
	{
		double[,] ArrMat = new double[3, 3] {   { 1.0, 4.0, 8.0 },
												{ 3.0, 2.0, 6.0 },
												{ 5.0, 9.0, 3.0 } };
		Matrix Mat = new Matrix(ArrMat);
		Matrix Ans = Mat.Inverse();

		Debug.LogFormat("Inverse of Matrix: \n{0}", Ans.ToString());
	}

	/*--------*/
	/*- Main -*/
	/*--------*/
	void Start () {
		HowToSetVector_case1();
		HowToSetVector_case2();
		HowToSetVector_case3();
		HowToSetVector_AllOne();
		HowToSetVector_Random();

		HowToSetMatrix_case1();
		HowToSetMatrix_case2();
		HowToSetMatrix_case3();
		HowToSetMatrix_AllOne();
		HowToSetMatrix_Identity();
		HowToSetMatrix_Random();

		AdditionOfVectors();
		SubtractionOfVectors();
		TranspositionOfVector();
		TranspositionOfMatrix();
		Multiplication_VectorT_Vector();
		Multiplication_Vector_VectorT();
		Multiplication_Matrix_Vector();
		DeterminantOfMatrix();
		InverseOfMatrix();
    }
}
